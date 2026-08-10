import { Badge, type BadgeTone } from "./Badge";
import { EmergencyAlertStatus, EmergencyAlertStatusLabels } from "../types/emergency";

const toneByStatus: Record<EmergencyAlertStatus, BadgeTone> = {
  [EmergencyAlertStatus.Active]: "red",
  [EmergencyAlertStatus.Resolved]: "emerald",
};

export function EmergencyAlertStatusBadge({ status }: { status: EmergencyAlertStatus }) {
  return <Badge label={EmergencyAlertStatusLabels[status]} tone={toneByStatus[status]} />;
}
