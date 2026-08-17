import { useMemo, useState } from "react";
import { setKpiItemFlags, submitKpiCheckpointScores } from "../api/kpi";
import { extractErrorMessage } from "../api/client";
import { StepForm, type WizardStep } from "./StepForm";
import { formatDate } from "../lib/format";
import type { KpiAppraisalDetailDto, KpiAppraisalItemDto } from "../types/kpi";

interface ScoreEntry {
  score: number | null;
  comment: string;
}

interface FlagEntry {
  inPlace: boolean | null;
  ability: boolean | null;
}

const scoreChipColor: Record<number, string> = {
  1: "bg-[#c65a5a] text-white border-[#c65a5a]",
  2: "bg-[#f5a83c] text-[#131415] border-[#f5a83c]",
  3: "bg-[#1ecb8f] text-[#131415] border-[#1ecb8f]",
};

function checkpointField(item: KpiAppraisalItemDto, checkpoint: number): { score: number | null; comment: string | null } {
  switch (checkpoint) {
    case 1:
      return { score: item.checkpoint1Score, comment: item.checkpoint1Comment };
    case 2:
      return { score: item.checkpoint2Score, comment: item.checkpoint2Comment };
    case 3:
      return { score: item.checkpoint3Score, comment: item.checkpoint3Comment };
    default:
      return { score: item.checkpoint4Score, comment: item.checkpoint4Comment };
  }
}

function checkpointDate(appraisal: KpiAppraisalDetailDto, checkpoint: number): string | null {
  switch (checkpoint) {
    case 1:
      return appraisal.checkpoint1Date;
    case 2:
      return appraisal.checkpoint2Date;
    case 3:
      return appraisal.checkpoint3Date;
    default:
      return appraisal.checkpoint4Date;
  }
}

interface KpiScoreEntryPanelProps {
  appraisal: KpiAppraisalDetailDto;
  onSaved: (updated: KpiAppraisalDetailDto) => void;
  onCancel: () => void;
}

export function KpiScoreEntryPanel({ appraisal, onSaved, onCancel }: KpiScoreEntryPanelProps) {
  const [checkpoint, setCheckpoint] = useState(1);
  const [scores, setScores] = useState<Record<string, ScoreEntry>>(() => buildScores(appraisal, 1));
  const [flags, setFlags] = useState<Record<string, FlagEntry>>(() => buildFlags(appraisal));
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  function buildScores(source: KpiAppraisalDetailDto, cp: number): Record<string, ScoreEntry> {
    const map: Record<string, ScoreEntry> = {};
    for (const item of source.items) {
      const field = checkpointField(item, cp);
      map[item.id] = { score: field.score, comment: field.comment ?? "" };
    }
    return map;
  }

  function buildFlags(source: KpiAppraisalDetailDto): Record<string, FlagEntry> {
    const map: Record<string, FlagEntry> = {};
    for (const item of source.items) {
      map[item.id] = { inPlace: item.inPlace, ability: item.ability };
    }
    return map;
  }

  function handleCheckpointChange(next: number) {
    setCheckpoint(next);
    setScores(buildScores(appraisal, next));
  }

  const categories = useMemo(() => {
    const order: string[] = [];
    const byCategory = new Map<string, KpiAppraisalItemDto[]>();
    for (const item of appraisal.items) {
      if (!byCategory.has(item.categoryName)) {
        byCategory.set(item.categoryName, []);
        order.push(item.categoryName);
      }
      byCategory.get(item.categoryName)!.push(item);
    }
    return order.map((name) => ({ name, items: byCategory.get(name)! }));
  }, [appraisal.items]);

  async function handleSubmit() {
    setError(null);
    setIsSubmitting(true);
    try {
      const scoreItems = Object.entries(scores)
        .filter(([, entry]) => entry.score !== null)
        .map(([itemId, entry]) => ({ itemId, score: entry.score!, comment: entry.comment || undefined }));

      let updated = appraisal;
      if (scoreItems.length > 0) {
        updated = await submitKpiCheckpointScores(appraisal.id, { checkpointNumber: checkpoint, items: scoreItems });
      }

      const flagItems = Object.entries(flags).map(([itemId, entry]) => ({
        itemId,
        inPlace: entry.inPlace,
        ability: entry.ability,
      }));
      updated = await setKpiItemFlags(appraisal.id, { items: flagItems });

      onSaved(updated);
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  const date = checkpointDate(appraisal, checkpoint);

  const steps: WizardStep[] = categories.map((category) => ({
    title: category.name,
    content: <div className="flex flex-col gap-3">{renderItems(category.items, scores, setScores, flags, setFlags)}</div>,
  }));

  return (
    <div className="rounded-2xl border border-white/10 bg-[#3a3d40]/90 p-6 shadow-2xl backdrop-blur-md font-['Inter']">
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h3 className="text-sm font-semibold text-white">
            Score {appraisal.employeeName} — {appraisal.periodLabel}
          </h3>
          <p className="text-xs text-white/50">{appraisal.designation}</p>
        </div>
        <div className="flex items-center gap-2">
          <div className="flex overflow-hidden rounded-lg border border-white/10">
            {[1, 2, 3, 4].map((cp) => (
              <button
                key={cp}
                type="button"
                onClick={() => handleCheckpointChange(cp)}
                className={`px-3 py-1.5 text-xs font-medium transition-colors ${
                  cp === checkpoint ? "bg-[#6fbe44] text-[#131415]" : "bg-transparent text-white/60 hover:bg-white/5"
                }`}
              >
                Review {cp}
              </button>
            ))}
          </div>
          {date && <span className="text-xs text-white/40">({formatDate(date)})</span>}
        </div>
      </div>

      <StepForm
        theme="dark"
        steps={steps}
        onSubmit={handleSubmit}
        submitLabel="Save scores"
        submittingLabel="Saving..."
        isSubmitting={isSubmitting}
        error={error}
      />

      <button type="button" onClick={onCancel} className="mt-3 text-xs text-white/40 hover:text-white/70 hover:underline">
        Close
      </button>
    </div>
  );
}

function renderItems(
  items: KpiAppraisalItemDto[],
  scores: Record<string, ScoreEntry>,
  setScores: React.Dispatch<React.SetStateAction<Record<string, ScoreEntry>>>,
  flags: Record<string, FlagEntry>,
  setFlags: React.Dispatch<React.SetStateAction<Record<string, FlagEntry>>>,
) {
  let previousSubGroup: string | null | undefined = undefined;
  const rows: React.ReactNode[] = [];

  for (const item of items) {
    if (item.subGroupLabel !== previousSubGroup) {
      previousSubGroup = item.subGroupLabel;
      if (item.subGroupLabel) {
        rows.push(
          <p key={`${item.id}-group`} className="mt-2 text-[10px] font-semibold uppercase tracking-wide text-white/40">
            {item.subGroupLabel}
          </p>,
        );
      }
    }

    const scoreEntry = scores[item.id] ?? { score: null, comment: "" };
    const flagEntry = flags[item.id] ?? { inPlace: null, ability: null };

    rows.push(
      <div
        key={item.id}
        className="rounded-lg border border-white/5 bg-[#2d2f31] p-3 transition-colors hover:border-[#6fbe44]/30"
      >
        <p className="text-sm text-white/90">{item.description}</p>
        <div className="mt-2 flex flex-wrap items-center gap-3">
          <div className="flex items-center gap-1">
            {[1, 2, 3].map((value) => (
              <button
                key={value}
                type="button"
                onClick={() =>
                  setScores((prev) => ({ ...prev, [item.id]: { ...scoreEntry, score: scoreEntry.score === value ? null : value } }))
                }
                className={`flex h-6 w-6 items-center justify-center rounded-full border text-[11px] font-semibold transition-colors ${
                  scoreEntry.score === value ? scoreChipColor[value] : "border-white/15 bg-white/5 text-white/50 hover:border-[#6fbe44]/40"
                }`}
              >
                {value}
              </button>
            ))}
          </div>
          <input
            type="text"
            placeholder="Comment (optional)"
            value={scoreEntry.comment}
            onChange={(e) => setScores((prev) => ({ ...prev, [item.id]: { ...scoreEntry, comment: e.target.value } }))}
            className="min-w-[180px] flex-1 rounded-lg border border-white/10 bg-[#202325] px-2 py-1 text-xs text-white placeholder:text-white/30 focus:border-[#6fbe44] focus:outline-none"
          />
          <button
            type="button"
            onClick={() => setFlags((prev) => ({ ...prev, [item.id]: { ...flagEntry, inPlace: !flagEntry.inPlace } }))}
            className={`rounded-full border px-2 py-0.5 text-[10px] font-medium transition-colors ${
              flagEntry.inPlace
                ? "border-[#6fbe44] bg-[#6fbe44]/15 text-[#93d75f]"
                : "border-white/15 bg-white/5 text-white/40 hover:border-white/30"
            }`}
          >
            In place
          </button>
          <button
            type="button"
            onClick={() => setFlags((prev) => ({ ...prev, [item.id]: { ...flagEntry, ability: !flagEntry.ability } }))}
            className={`rounded-full border px-2 py-0.5 text-[10px] font-medium transition-colors ${
              flagEntry.ability
                ? "border-[#6fbe44] bg-[#6fbe44]/15 text-[#93d75f]"
                : "border-white/15 bg-white/5 text-white/40 hover:border-white/30"
            }`}
          >
            Ability
          </button>
        </div>
      </div>,
    );
  }

  return rows;
}
