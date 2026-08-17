import { useEffect, useState } from "react";
import { AlertTriangle, CalendarOff, GraduationCap, HeartPulse, TrendingUp } from "lucide-react";
import { useAuth } from "../auth/AuthContext";
import { getDashboardSummary } from "../api/dashboard";
import { getMyNotifications } from "../api/notifications";
import { extractErrorMessage } from "../api/client";
import { StatCard } from "../components/StatCard";
import { ModuleLaunchGrid } from "../components/ModuleLaunchGrid";
import { Card } from "../components/Card";
import { StatusBadge } from "../components/StatusBadge";
import { formatDate, formatDateTime } from "../lib/format";
import { useCountUp } from "../lib/useCountUp";
import { EmployeeRole } from "../types/auth";
import { ShiftTypeLabels } from "../types/workShift";
import type { DashboardSummary } from "../types/dashboard";
import type { NotificationDto } from "../types/notification";

function formatPercent(value: number | null): string {
  return value === null ? "—" : `${value}%`;
}

export function DashboardPage() {
  const { session } = useAuth();
  const firstName = session?.fullName.split(" ")[0] ?? "there";
  const isLineManager = session?.role === EmployeeRole.LineManager;

  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [activity, setActivity] = useState<NotificationDto[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getDashboardSummary()
      .then(setSummary)
      .catch((err) => setError(extractErrorMessage(err)));
    getMyNotifications()
      .then((data) => setActivity(data.slice(0, 6)))
      .catch(() => {
        // Non-fatal — the activity feed just stays empty.
      });
  }, []);

  const loadingDetail = error ?? "Loading...";

  const incidentsCount = useCountUp(summary ? summary.incidentsThisMonth : null);
  const medicalPercent = useCountUp(summary?.medicalCompliancePercent ?? null, 1);
  const trainingPercent = useCountUp(summary?.trainingCompliancePercent ?? null, 1);
  const pendingLeave = useCountUp(summary ? summary.pendingLeaveCount : null);
  const kpiScore = useCountUp(summary?.myKpiOverallScorePercent ?? null, 1);

  return (
    <div className="stagger-children flex flex-col gap-6">
      <div className="circuit-texture relative overflow-hidden rounded-xl bg-slate-950 px-6 py-6 shadow-lg">
        <div className="gradient-glow" />
        <div className="relative z-10">
          <h2 className="text-xl font-semibold text-white">Welcome back, {firstName}</h2>
          <p className="text-sm text-slate-400">Here's what's happening today.</p>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5">
        <StatCard
          label="Incidents (MTD)"
          value={incidentsCount !== null ? String(incidentsCount) : "—"}
          detail={summary ? "Reported this month, org-wide" : loadingDetail}
          icon={AlertTriangle}
          iconTone={summary && summary.incidentsThisMonth > 0 ? "red" : "emerald"}
          tone={summary && summary.incidentsThisMonth > 0 ? "warning" : "good"}
        />
        <StatCard
          label="Medical Compliance"
          value={formatPercent(medicalPercent)}
          detail={summary ? "Employees with a current fit status" : loadingDetail}
          icon={HeartPulse}
          iconTone="violet"
          tone={
            summary?.medicalCompliancePercent != null && summary.medicalCompliancePercent < 80 ? "warning" : "default"
          }
        />
        <StatCard
          label="Training Compliance"
          value={formatPercent(trainingPercent)}
          detail={summary ? "Certificates not yet expired" : loadingDetail}
          icon={GraduationCap}
          iconTone="emerald"
          tone={
            summary?.trainingCompliancePercent != null && summary.trainingCompliancePercent < 80 ? "warning" : "default"
          }
        />
        <StatCard
          label="Leave Requests"
          value={pendingLeave !== null ? String(pendingLeave) : "—"}
          detail={summary ? (isLineManager ? "Pending your approval" : "Your pending requests") : loadingDetail}
          icon={CalendarOff}
          iconTone="amber"
          tone={summary && summary.pendingLeaveCount > 0 ? "warning" : "default"}
        />
        <StatCard
          label="KPI Score"
          value={formatPercent(kpiScore)}
          detail={summary ? "Your latest KPI appraisal" : loadingDetail}
          icon={TrendingUp}
          iconTone="blue"
        />
      </div>

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <Card title="Recent Shift Requests">
          {!summary || summary.recentShiftRequests.length === 0 ? (
            <p className="px-6 py-8 text-center text-sm text-slate-500">
              {summary ? "No shift requests yet." : loadingDetail}
            </p>
          ) : (
            <ul className="divide-y divide-slate-100">
              {summary.recentShiftRequests.map((request) => (
                <li key={request.id} className="flex items-center justify-between gap-3 px-6 py-3">
                  <div className="min-w-0">
                    <p className="truncate text-sm font-medium text-slate-900">{request.employeeName}</p>
                    <p className="text-xs text-slate-500">
                      {formatDate(request.requestedShiftDate)} — {ShiftTypeLabels[request.requestedShiftType]}
                    </p>
                  </div>
                  <StatusBadge status={request.status} />
                </li>
              ))}
            </ul>
          )}
        </Card>

        <Card title="Latest Activity">
          {activity.length === 0 ? (
            <p className="px-6 py-8 text-center text-sm text-slate-500">No recent activity.</p>
          ) : (
            <ul className="divide-y divide-slate-100">
              {activity.map((entry) => (
                <li key={entry.id} className="px-6 py-3">
                  <p className="text-sm text-slate-700">{entry.message}</p>
                  <p className="text-xs text-slate-400">{formatDateTime(entry.createdAtUtc)}</p>
                </li>
              ))}
            </ul>
          )}
        </Card>
      </div>

      <ModuleLaunchGrid />
    </div>
  );
}
