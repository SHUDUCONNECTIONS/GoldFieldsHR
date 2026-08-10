export const EmergencyAlertStatus = {
  Active: 0,
  Resolved: 1,
} as const;
export type EmergencyAlertStatus = (typeof EmergencyAlertStatus)[keyof typeof EmergencyAlertStatus];

export const EmergencyAlertStatusLabels: Record<EmergencyAlertStatus, string> = {
  [EmergencyAlertStatus.Active]: "Active",
  [EmergencyAlertStatus.Resolved]: "Resolved",
};

export interface EmergencyAlertDto {
  id: string;
  employeeId: string;
  employeeName: string;
  location: string;
  message: string | null;
  status: EmergencyAlertStatus;
  triggeredAtUtc: string;
  resolvedAtUtc: string | null;
  resolutionNotes: string | null;
}

export interface TriggerEmergencyAlertRequest {
  location: string;
  message?: string;
}

export interface ResolveEmergencyAlertRequest {
  resolutionNotes?: string;
}
