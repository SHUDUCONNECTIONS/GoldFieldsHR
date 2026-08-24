export const AttachmentEntityType = {
  Policy: 0,
  Certificate: 1,
  MedicalExamination: 2,
  LeaveRequest: 4,
  LegalAppointment: 5,
  BoardTask: 6,
  WorkShiftSchedule: 7,
} as const;
export type AttachmentEntityType = (typeof AttachmentEntityType)[keyof typeof AttachmentEntityType];

export interface AttachmentDto {
  id: string;
  entityType: AttachmentEntityType;
  entityId: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  uploadedByName: string;
  uploadedAtUtc: string;
}
