import { useCallback, useEffect, useMemo, useState } from "react";
import { AlertTriangle, CalendarDays, CheckCircle2, Clock, LayoutGrid, Trophy, Users } from "lucide-react";
import { useAuth } from "../auth/AuthContext";
import { getCompletedBoards, getMyPerformance, getOrgPerformance, getOrgPerformanceSummary } from "../api/performance";
import { getAllSites } from "../api/sites";
import { extractErrorMessage } from "../api/client";
import { PersonAvatar } from "../components/boards/PersonAvatar";
import { KpiSkeletonCard, KpiSkeletonStat } from "../components/KpiSkeletons";
import { useCountUp } from "../lib/useCountUp";
import { formatDate } from "../lib/format";
import { BoardPriority, BoardPriorityColors, BoardPriorityLabels } from "../types/board";
import { EmployeeRole } from "../types/auth";
import {
  PerformanceRange,
  PerformanceRangeLabels,
  type CompletedBoardDto,
  type EmployeePerformanceDto,
  type MyPerformanceDto,
  type OrgPerformanceSummaryDto,
} from "../types/performance";
import type { SiteAdminDto } from "../types/site";

const RANGE_OPTIONS = [PerformanceRange.Week, PerformanceRange.Month, PerformanceRange.All];

function RangeToggle({ range, onChange }: { range: PerformanceRange; onChange: (range: PerformanceRange) => void }) {
  return (
    <div className="flex gap-1.5 rounded-lg border border-white/10 bg-[#3a3d40] p-1">
      {RANGE_OPTIONS.map((option) => (
        <button
          key={option}
          type="button"
          onClick={() => onChange(option)}
          className={`rounded-md px-3 py-1.5 text-xs font-medium transition-colors ${
            range === option ? "bg-[#3f7429] text-[#eaf6de]" : "text-white/50 hover:bg-white/5"
          }`}
        >
          {PerformanceRangeLabels[option]}
        </button>
      ))}
    </div>
  );
}

interface StatPillProps {
  label: string;
  value: number | null;
  icon: typeof Clock;
  color: string;
}

function StatPill({ label, value, icon: Icon, color }: StatPillProps) {
  const animated = useCountUp(value);
  return (
    <div
      className="flex items-center gap-2 rounded-3xl border px-4 py-2"
      style={{ backgroundColor: `${color}1f`, borderColor: `${color}40` }}
    >
      <Icon className="h-4 w-4" style={{ color }} />
      <span className="text-sm font-semibold" style={{ color }}>
        {animated ?? "—"}
      </span>
      <span className="text-xs text-white/50">{label}</span>
    </div>
  );
}

function PerformanceChart({ chart }: { chart: MyPerformanceDto["chart"] }) {
  const max = Math.max(1, ...chart.map((point) => point.tasksCompleted));
  const isDense = chart.length > 10;

  return (
    <div className="overflow-x-auto rounded-xl border border-white/10 bg-[#3a3d40] p-4">
      <div className="flex items-end gap-2" style={{ minWidth: chart.length > 14 ? chart.length * 28 : undefined }}>
        {chart.map((point) => (
          <div key={point.bucketStart} className="flex flex-1 flex-col items-center gap-1.5" style={{ minWidth: isDense ? 20 : undefined }}>
            {!isDense && <span className="text-[10px] font-semibold text-white/70">{point.tasksCompleted}</span>}
            <div className="flex h-32 w-full items-end overflow-hidden rounded-md bg-white/5" title={`${point.label}: ${point.tasksCompleted}`}>
              <div
                className="w-full rounded-md bg-[#6fbe44] transition-all duration-500"
                style={{ height: `${Math.max(4, (point.tasksCompleted / max) * 100)}%` }}
              />
            </div>
            <span className="whitespace-nowrap text-[10px] text-white/40">{point.label}</span>
          </div>
        ))}
      </div>
    </div>
  );
}

function OrgPerformanceRow({ row, maxCompleted }: { row: EmployeePerformanceDto; maxCompleted: number }) {
  const barWidth = Math.max(4, (row.tasksCompleted / maxCompleted) * 100);
  return (
    <tr className="transition-colors hover:bg-[#454850]">
      <td className="px-4 py-3">
        <div className="flex items-center gap-2">
          <PersonAvatar name={row.employeeName} size={28} />
          <div>
            <p className="font-medium text-white">{row.employeeName}</p>
            <p className="text-[11px] text-white/40">{row.siteName}</p>
          </div>
        </div>
      </td>
      <td className="px-4 py-3">
        <div className="flex items-center gap-2">
          <div className="h-1.5 w-24 overflow-hidden rounded-full bg-white/10">
            <div className="h-full rounded-full bg-[#6fbe44]" style={{ width: `${barWidth}%` }} />
          </div>
          <span className="text-white/80">{row.tasksCompleted}</span>
        </div>
      </td>
      <td className="px-4 py-3 text-[#93d75f]">{row.tasksDoneThisWeek}</td>
      <td className="px-4 py-3 text-[#f5a83c]">{row.tasksInProgress}</td>
      <td className="px-4 py-3 text-[#e69c9c]">{row.tasksOverdue}</td>
      <td className="px-4 py-3 text-white/70">{row.boardsCompleted}</td>
      <td className="px-4 py-3">
        <span
          className={`font-semibold ${
            row.completionRatePercent >= 75 ? "text-[#1ecb8f]" : row.completionRatePercent >= 40 ? "text-[#f5a83c]" : "text-[#e69c9c]"
          }`}
        >
          {row.completionRatePercent}%
        </span>
      </td>
    </tr>
  );
}

interface SummaryTileProps {
  label: string;
  value: number | string | null;
  sub?: string;
  icon: typeof Clock;
  color: string;
}

function SummaryTile({ label, value, sub, icon: Icon, color }: SummaryTileProps) {
  const isNumeric = typeof value === "number";
  const animated = useCountUp(isNumeric ? value : null);
  return (
    <div
      className="flex min-w-[160px] flex-1 items-center gap-3 rounded-xl border-l-4 bg-[#3a3d40] px-4 py-3"
      style={{ borderLeftColor: color }}
    >
      <Icon className="h-6 w-6 shrink-0" style={{ color }} />
      <div className="min-w-0">
        <p className="truncate text-[11px] text-white/40">{label}</p>
        <p className="truncate text-lg font-bold text-white">{isNumeric ? (animated ?? "—") : value ?? "—"}</p>
        {sub && <p className="truncate text-[11px] text-white/40">{sub}</p>}
      </div>
    </div>
  );
}

function CompletedBoardCard({ board }: { board: CompletedBoardDto }) {
  const priorityColor = BoardPriorityColors[board.priority];
  return (
    <div className="flex flex-col gap-2 rounded-xl border-l-4 border-l-[#1ecb8f] bg-[#3a3d40] p-4">
      <div className="flex items-start justify-between gap-2">
        <p className="text-sm font-semibold text-white">{board.name}</p>
        <span className="shrink-0 rounded-full bg-[#1ecb8f]/15 px-2 py-0.5 text-[10px] font-semibold text-[#1ecb8f]">
          Completed
        </span>
      </div>
      {board.description && <p className="line-clamp-2 text-xs text-white/50">{board.description}</p>}
      <div className="flex flex-wrap items-center gap-1.5 text-[10px] text-white/40">
        <span>
          Owned by <strong className="text-white/70">{board.ownerEmployeeName}</strong>
        </span>
        {board.deadline && (
          <span className="flex items-center gap-1">
            <CalendarDays className="h-2.5 w-2.5" />
            Deadline: {formatDate(board.deadline)}
          </span>
        )}
        {board.priority !== BoardPriority.Normal && (
          <span className="rounded-full px-1.5 py-0.5 font-semibold" style={{ backgroundColor: `${priorityColor}26`, color: priorityColor }}>
            {BoardPriorityLabels[board.priority]}
          </span>
        )}
      </div>
      {board.memberNames.length > 0 && (
        <div className="flex items-center gap-1.5 pt-1">
          <div className="flex -space-x-2">
            {board.memberNames.slice(0, 5).map((name) => (
              <PersonAvatar key={name} name={name} size={22} className="border-[#3a3d40]" />
            ))}
          </div>
          <span className="text-[10px] text-white/40">
            {board.memberNames.length} member{board.memberNames.length !== 1 ? "s" : ""}
          </span>
        </div>
      )}
    </div>
  );
}

export function PerformancePage() {
  const { session } = useAuth();
  const isHR = session?.role === EmployeeRole.HR;
  const isExecutive = session?.role === EmployeeRole.Executive;
  const isOrgView = isHR || isExecutive;

  const [range, setRange] = useState<PerformanceRange>(PerformanceRange.Week);
  const [myPerformance, setMyPerformance] = useState<MyPerformanceDto | null>(null);
  const [orgPerformance, setOrgPerformance] = useState<EmployeePerformanceDto[]>([]);
  const [orgSummary, setOrgSummary] = useState<OrgPerformanceSummaryDto | null>(null);
  const [completedBoards, setCompletedBoards] = useState<CompletedBoardDto[]>([]);
  const [sites, setSites] = useState<SiteAdminDto[]>([]);
  const [selectedSiteId, setSelectedSiteId] = useState<string>("");
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadMine = useCallback(async (activeRange: PerformanceRange) => {
    try {
      setMyPerformance(await getMyPerformance(activeRange));
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsLoading(false);
    }
  }, []);

  const loadOrg = useCallback(async (activeRange: PerformanceRange, siteId: string) => {
    try {
      const [performance, summary, boards] = await Promise.all([
        getOrgPerformance(activeRange, siteId || undefined),
        getOrgPerformanceSummary(siteId || undefined),
        getCompletedBoards(siteId || undefined),
      ]);
      setOrgPerformance(performance);
      setOrgSummary(summary);
      setCompletedBoards(boards);
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    setIsLoading(true);
    if (isOrgView) {
      loadOrg(range, selectedSiteId);
    } else {
      loadMine(range);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isOrgView, range, selectedSiteId]);

  useEffect(() => {
    if (isExecutive) {
      getAllSites()
        .then(setSites)
        .catch((err) => setError(extractErrorMessage(err)));
    }
  }, [isExecutive]);

  const maxCompleted = useMemo(() => Math.max(1, ...orgPerformance.map((r) => r.tasksCompleted)), [orgPerformance]);
  const sortedOrgPerformance = useMemo(
    () => [...orgPerformance].sort((a, b) => b.tasksCompleted - a.tasksCompleted),
    [orgPerformance],
  );

  return (
    <div className="stagger-children flex flex-col gap-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h2 className="text-lg font-semibold text-white">Performance</h2>
        <div className="flex flex-wrap items-center gap-2">
          {isExecutive && (
            <select
              value={selectedSiteId}
              onChange={(e) => setSelectedSiteId(e.target.value)}
              className="rounded-lg border border-white/10 bg-[#3a3d40] px-3 py-2 text-sm text-white focus:border-[#6fbe44] focus:outline-none"
            >
              <option value="">All sites</option>
              {sites.map((site) => (
                <option key={site.id} value={site.id}>
                  {site.name}
                </option>
              ))}
            </select>
          )}
          <RangeToggle range={range} onChange={setRange} />
        </div>
      </div>

      {error && <p className="text-sm text-[#e69c9c]">{error}</p>}

      {!isOrgView &&
        (isLoading ? (
          <div className="flex flex-wrap gap-2">
            <KpiSkeletonStat />
            <KpiSkeletonStat />
            <KpiSkeletonStat />
          </div>
        ) : (
          myPerformance && (
            <>
              <div className="flex flex-wrap gap-2">
                <StatPill label="Completed" value={myPerformance.tasksCompletedTotal} icon={CheckCircle2} color="#1ecb8f" />
                <StatPill label="In progress" value={myPerformance.tasksInProgress} icon={Clock} color="#f5a83c" />
                <StatPill label="Overdue" value={myPerformance.tasksOverdue} icon={AlertTriangle} color="#c65a5a" />
              </div>
              <PerformanceChart chart={myPerformance.chart} />
            </>
          )
        ))}

      {isOrgView && (
        <>
          <section className="flex flex-wrap gap-3">
            {isLoading || !orgSummary ? (
              <>
                <KpiSkeletonStat />
                <KpiSkeletonStat />
                <KpiSkeletonStat />
                <KpiSkeletonStat />
                <KpiSkeletonStat />
              </>
            ) : (
              <>
                <SummaryTile label="Tasks Done This Week" value={orgSummary.tasksDoneThisWeek} icon={CheckCircle2} color="#1ecb8f" />
                <SummaryTile label="In Progress Across Team" value={orgSummary.tasksInProgress} icon={Clock} color="#f5a83c" />
                <SummaryTile label="Team Members" value={orgSummary.teamMembers} icon={Users} color="#f5a83c" />
                <SummaryTile label="Boards Completed (All Time)" value={orgSummary.boardsCompletedAllTime} icon={LayoutGrid} color="#6fbe44" />
                <SummaryTile
                  label="Top Performer This Week"
                  value={orgSummary.topPerformerName ?? "—"}
                  sub={orgSummary.topPerformerName ? `${orgSummary.topPerformerTasksDoneThisWeek} tasks done` : undefined}
                  icon={Trophy}
                  color="#ffc107"
                />
              </>
            )}
          </section>

          <section className="flex flex-col gap-3">
            <h3 className="text-sm font-semibold text-white">
              {isExecutive ? "Employee performance" : "All employees"}
            </h3>
            {isLoading ? (
              <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
                <KpiSkeletonCard />
                <KpiSkeletonCard />
              </div>
            ) : sortedOrgPerformance.length === 0 ? (
              <p className="rounded-xl border border-white/10 bg-[#3a3d40] px-6 py-8 text-center text-sm text-white/40">
                No employees found.
              </p>
            ) : (
              <div className="overflow-hidden rounded-xl border border-white/10">
                <div className="overflow-x-auto">
                  <table className="w-full min-w-[760px] text-left text-xs">
                    <thead>
                      <tr className="bg-[#3f7429] text-white">
                        <th className="px-4 py-3 font-semibold">Employee</th>
                        <th className="px-4 py-3 font-semibold">Completed</th>
                        <th className="px-4 py-3 font-semibold">Done This Week</th>
                        <th className="px-4 py-3 font-semibold">In Progress</th>
                        <th className="px-4 py-3 font-semibold">Overdue</th>
                        <th className="px-4 py-3 font-semibold">Boards Completed</th>
                        <th className="px-4 py-3 font-semibold">Completion Rate</th>
                      </tr>
                    </thead>
                    <tbody className="bg-[#3a3d40]">
                      {sortedOrgPerformance.map((row) => (
                        <OrgPerformanceRow key={row.employeeId} row={row} maxCompleted={maxCompleted} />
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}
          </section>

          <section className="flex flex-col gap-3">
            <h3 className="text-sm font-semibold text-white">
              Completed boards history
              <span className="ml-2 rounded-full bg-[#1ecb8f]/15 px-2 py-0.5 text-[11px] font-semibold text-[#1ecb8f]">
                {completedBoards.length}
              </span>
            </h3>
            {isLoading ? (
              <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
                <KpiSkeletonCard />
              </div>
            ) : completedBoards.length === 0 ? (
              <p className="rounded-xl border border-dashed border-white/10 bg-[#3a3d40] px-6 py-8 text-center text-sm text-white/40">
                No boards have been completed yet.
              </p>
            ) : (
              <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
                {completedBoards.map((board) => (
                  <CompletedBoardCard key={board.id} board={board} />
                ))}
              </div>
            )}
          </section>
        </>
      )}
    </div>
  );
}
