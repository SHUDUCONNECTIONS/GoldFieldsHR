import { Badge, type BadgeTone } from "./Badge";
import { PpeRequestStatus, PpeRequestStatusLabels } from "../types/ppe";

const toneByStatus: Record<PpeRequestStatus, BadgeTone> = {
  [PpeRequestStatus.Pending]: "amber",
  [PpeRequestStatus.Approved]: "amber",
  [PpeRequestStatus.Rejected]: "red",
  [PpeRequestStatus.Issued]: "emerald",
};

export function PpeStatusBadge({ status }: { status: PpeRequestStatus }) {
  return <Badge label={PpeRequestStatusLabels[status]} tone={toneByStatus[status]} />;
}
