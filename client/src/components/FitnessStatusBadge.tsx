import { Badge, type BadgeTone } from "./Badge";
import { FitnessStatus, FitnessStatusLabels } from "../types/medical";

const toneByStatus: Record<FitnessStatus, BadgeTone> = {
  [FitnessStatus.Fit]: "emerald",
  [FitnessStatus.FitWithRestrictions]: "amber",
  [FitnessStatus.Unfit]: "red",
  [FitnessStatus.Pending]: "amber",
};

export function FitnessStatusBadge({ status }: { status: FitnessStatus }) {
  return <Badge label={FitnessStatusLabels[status]} tone={toneByStatus[status]} />;
}
