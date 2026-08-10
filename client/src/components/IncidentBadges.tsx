import { Badge, type BadgeTone } from "./Badge";
import {
  IncidentSeverity,
  IncidentSeverityLabels,
  IncidentStatus,
  IncidentStatusLabels,
} from "../types/incident";

const severityTone: Record<IncidentSeverity, BadgeTone> = {
  [IncidentSeverity.Low]: "emerald",
  [IncidentSeverity.Medium]: "amber",
  [IncidentSeverity.High]: "red",
  [IncidentSeverity.Critical]: "red",
};

const statusTone: Record<IncidentStatus, BadgeTone> = {
  [IncidentStatus.Reported]: "amber",
  [IncidentStatus.UnderInvestigation]: "amber",
  [IncidentStatus.Closed]: "emerald",
};

export function IncidentSeverityBadge({ severity }: { severity: IncidentSeverity }) {
  return <Badge label={IncidentSeverityLabels[severity]} tone={severityTone[severity]} />;
}

export function IncidentStatusBadge({ status }: { status: IncidentStatus }) {
  return <Badge label={IncidentStatusLabels[status]} tone={statusTone[status]} />;
}
