import type { ReactNode } from "react";
import { KpiAvatar } from "./KpiAvatar";
import { KpiProgressRing } from "./KpiProgressRing";
import { KpiStatusBadge } from "./KpiStatusBadge";
import { downloadKpiAppraisalPdf } from "../api/kpi";
import { extractErrorMessage } from "../api/client";
import { formatDateTime } from "../lib/format";
import { KpiAppraisalStatus, type KpiAppraisalSummaryDto } from "../types/kpi";

function scorePercentColor(percent: number | null): string {
  if (percent === null) return "bg-white/15";
  if (percent >= 80) return "bg-[#1ecb8f]";
  if (percent >= 50) return "bg-[#f5a83c]";
  return "bg-[#c65a5a]";
}

function accentBorderColor(status: KpiAppraisalStatus): string {
  switch (status) {
    case KpiAppraisalStatus.Finalized:
      return "border-l-[#1ecb8f]";
    case KpiAppraisalStatus.PendingBlastingEngineerSignOff:
      return "border-l-[#f5a83c]";
    default:
      return "border-l-white/20";
  }
}

function CategoryBar({ name, scorePercent }: { name: string; scorePercent: number | null }) {
  return (
    <div className="flex items-center gap-2">
      <span className="w-36 shrink-0 truncate text-[11px] text-white/60">{name}</span>
      <div className="h-1.5 flex-1 overflow-hidden rounded-full border border-white/10 bg-white/5">
        <div
          className={`h-full rounded-full transition-[width] duration-300 ${scorePercentColor(scorePercent)}`}
          style={{ width: `${scorePercent ?? 0}%` }}
        />
      </div>
      <span className="w-9 shrink-0 text-right text-[11px] text-white/50">
        {scorePercent === null ? "—" : `${scorePercent}%`}
      </span>
    </div>
  );
}

interface KpiAppraisalSummaryCardProps {
  appraisal: KpiAppraisalSummaryDto;
  onError?: (message: string) => void;
  actions?: ReactNode;
  /** Full-width content rendered below the category bars (e.g. a sign-off panel). */
  footer?: ReactNode;
}

export function KpiAppraisalSummaryCard({ appraisal, onError, actions, footer }: KpiAppraisalSummaryCardProps) {
  async function handleDownload() {
    try {
      await downloadKpiAppraisalPdf(appraisal.id, `kpi-appraisal-${appraisal.employeeName}-${appraisal.periodLabel}.pdf`);
    } catch (err) {
      onError?.(extractErrorMessage(err));
    }
  }

  return (
    <div
      className={`flex flex-col gap-3 rounded-xl border-l-4 ${accentBorderColor(appraisal.status)} bg-[#3a3d40] p-4 shadow-lg transition-all duration-200 hover:-translate-y-1 hover:shadow-2xl`}
    >
      <div className="flex items-start justify-between gap-3">
        <div className="flex min-w-0 items-center gap-2.5">
          <KpiAvatar name={appraisal.employeeName} />
          <div className="min-w-0">
            <p className="truncate text-sm font-semibold text-white">{appraisal.employeeName}</p>
            <p className="truncate text-xs text-white/50">
              {appraisal.designation} — {appraisal.periodLabel}
            </p>
            <div className="mt-1">
              <KpiStatusBadge status={appraisal.status} />
            </div>
          </div>
        </div>
        <KpiProgressRing percent={appraisal.overallScorePercent} />
      </div>

      {appraisal.categories.length > 0 && (
        <div className="flex flex-col gap-1.5">
          {appraisal.categories.map((category) => (
            <CategoryBar key={category.name} name={category.name} scorePercent={category.scorePercent} />
          ))}
        </div>
      )}

      <p className="text-[11px] text-white/40">
        {appraisal.lastReviewedAtUtc ? `Last scored ${formatDateTime(appraisal.lastReviewedAtUtc)}` : "Not scored yet"}
        {appraisal.signedOffBy.length > 0 && ` · Signed by ${appraisal.signedOffBy.join(", ")}`}
      </p>

      <div className="flex flex-wrap items-center gap-2">
        <button
          type="button"
          onClick={handleDownload}
          className="rounded-lg border border-[#6fbe44]/40 px-3 py-1.5 text-xs font-medium text-[#93d75f] transition-colors hover:bg-[#6fbe44]/10"
        >
          PDF
        </button>
        {actions}
      </div>

      {footer}
    </div>
  );
}
