import { Badge, type BadgeTone } from "./Badge";
import { ShiftRequestStatus, ShiftRequestStatusLabels } from "../types/workShift";

const toneByStatus: Record<ShiftRequestStatus, BadgeTone> = {
  [ShiftRequestStatus.PendingLineManagerApproval]: "amber",
  [ShiftRequestStatus.PendingHRApproval]: "amber",
  [ShiftRequestStatus.Approved]: "emerald",
  [ShiftRequestStatus.Rejected]: "red",
};

export function StatusBadge({ status }: { status: ShiftRequestStatus }) {
  return <Badge label={ShiftRequestStatusLabels[status]} tone={toneByStatus[status]} />;
}
