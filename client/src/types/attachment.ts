export const AttachmentEntityType = {
  Policy: 0,
  Certificate: 1,
  MedicalExamination: 2,
  IncidentReport: 3,
  LeaveRequest: 4,
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
