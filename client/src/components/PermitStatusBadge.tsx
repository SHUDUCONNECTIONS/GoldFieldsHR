import { Badge, type BadgeTone } from "./Badge";
import { PermitStatus, PermitStatusLabels } from "../types/permit";

const toneByStatus: Record<PermitStatus, BadgeTone> = {
  [PermitStatus.Pending]: "amber",
  [PermitStatus.Approved]: "amber",
  [PermitStatus.Rejected]: "red",
  [PermitStatus.Closed]: "emerald",
};

export function PermitStatusBadge({ status }: { status: PermitStatus }) {
  return <Badge label={PermitStatusLabels[status]} tone={toneByStatus[status]} />;
}
