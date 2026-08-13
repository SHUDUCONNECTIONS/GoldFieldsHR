import { Badge, type BadgeTone } from "./Badge";
import { LegalAppointmentStatus, LegalAppointmentStatusLabels } from "../types/legalAppointment";

const toneByStatus: Record<LegalAppointmentStatus, BadgeTone> = {
  [LegalAppointmentStatus.Pending]: "amber",
  [LegalAppointmentStatus.Active]: "emerald",
  [LegalAppointmentStatus.Rejected]: "red",
  [LegalAppointmentStatus.Revoked]: "slate",
};

export function LegalAppointmentStatusBadge({ status }: { status: LegalAppointmentStatus }) {
  return <Badge label={LegalAppointmentStatusLabels[status]} tone={toneByStatus[status]} />;
}
