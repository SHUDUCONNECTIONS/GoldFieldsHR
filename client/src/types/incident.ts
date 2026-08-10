export const IncidentSeverity = {
  Low: 0,
  Medium: 1,
  High: 2,
  Critical: 3,
} as const;
export type IncidentSeverity = (typeof IncidentSeverity)[keyof typeof IncidentSeverity];

export const IncidentSeverityLabels: Record<IncidentSeverity, string> = {
  [IncidentSeverity.Low]: "Low",
  [IncidentSeverity.Medium]: "Medium",
  [IncidentSeverity.High]: "High",
  [IncidentSeverity.Critical]: "Critical",
};

export const IncidentStatus = {
  Reported: 0,
  UnderInvestigation: 1,
  Closed: 2,
} as const;
export type IncidentStatus = (typeof IncidentStatus)[keyof typeof IncidentStatus];

export const IncidentStatusLabels: Record<IncidentStatus, string> = {
  [IncidentStatus.Reported]: "Reported",
  [IncidentStatus.UnderInvestigation]: "Under Investigation",
  [IncidentStatus.Closed]: "Closed",
};

export interface IncidentReportDto {
  id: string;
  employeeId: string;
  employeeName: string;
  title: string;
  description: string;
  severity: IncidentSeverity;
  location: string;
  occurredAtUtc: string;
  status: IncidentStatus;
  createdAtUtc: string;
  reviewedAtUtc: string | null;
  reviewNotes: string | null;
}

export interface SubmitIncidentReportRequest {
  title: string;
  description: string;
  severity: IncidentSeverity;
  location: string;
  occurredAtUtc: string;
}

export interface UpdateIncidentStatusRequest {
  status: IncidentStatus;
  reviewNotes?: string;
}
