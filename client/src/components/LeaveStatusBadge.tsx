import { Badge, type BadgeTone } from "./Badge";
import { LeaveRequestStatus, LeaveRequestStatusLabels } from "../types/leave";

const toneByStatus: Record<LeaveRequestStatus, BadgeTone> = {
  [LeaveRequestStatus.PendingLineManagerApproval]: "amber",
  [LeaveRequestStatus.PendingHRApproval]: "amber",
  [LeaveRequestStatus.Approved]: "emerald",
  [LeaveRequestStatus.Rejected]: "red",
};

export function LeaveStatusBadge({ status }: { status: LeaveRequestStatus }) {
  return <Badge label={LeaveRequestStatusLabels[status]} tone={toneByStatus[status]} />;
}
