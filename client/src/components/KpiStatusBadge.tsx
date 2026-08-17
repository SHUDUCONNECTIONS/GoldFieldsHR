import { KpiAppraisalStatus, KpiAppraisalStatusLabels } from "../types/kpi";

// Collabrio chip tokens: small pill, alpha-tinted background + matching border/text.
const toneByStatus: Record<KpiAppraisalStatus, { bg: string; text: string; dot: string }> = {
  [KpiAppraisalStatus.InProgress]: { bg: "bg-white/10", text: "text-white/70", dot: "bg-white/40" },
  [KpiAppraisalStatus.PendingBlastingEngineerSignOff]: { bg: "bg-[#f5a83c]/15", text: "text-[#f5a83c]", dot: "bg-[#f5a83c]" },
  [KpiAppraisalStatus.Finalized]: { bg: "bg-[#1ecb8f]/15", text: "text-[#1ecb8f]", dot: "bg-[#1ecb8f]" },
};

export function KpiStatusBadge({ status }: { status: KpiAppraisalStatus }) {
  const tone = toneByStatus[status];
  return (
    <span className={`inline-flex items-center gap-1.5 rounded-full px-2 py-0.5 text-[10px] font-semibold ${tone.bg} ${tone.text}`}>
      <span className={`h-1.5 w-1.5 rounded-full ${tone.dot}`} />
      {KpiAppraisalStatusLabels[status]}
    </span>
  );
}
