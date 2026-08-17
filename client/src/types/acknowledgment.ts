export const AcknowledgmentEntityType = {
  IncidentReport: 0,
  PreShiftSafetyCheck: 1,
  PpeRequest: 2,
  LegalAppointment: 3,
} as const;
export type AcknowledgmentEntityType = (typeof AcknowledgmentEntityType)[keyof typeof AcknowledgmentEntityType];

export interface AcknowledgmentDto {
  id: string;
  entityType: AcknowledgmentEntityType;
  entityId: string;
  employeeId: string;
  employeeName: string;
  createdAtUtc: string;
}
