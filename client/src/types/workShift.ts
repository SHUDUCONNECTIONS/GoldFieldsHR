export const ShiftType = {
  Day: 0,
  Night: 1,
} as const;
export type ShiftType = (typeof ShiftType)[keyof typeof ShiftType];

export const ShiftTypeLabels: Record<ShiftType, string> = {
  [ShiftType.Day]: "Day",
  [ShiftType.Night]: "Night",
};

export const ShiftRequestStatus = {
  PendingLineManagerApproval: 0,
  PendingHRApproval: 1,
  Approved: 2,
  Rejected: 3,
} as const;
export type ShiftRequestStatus = (typeof ShiftRequestStatus)[keyof typeof ShiftRequestStatus];

export const ShiftRequestStatusLabels: Record<ShiftRequestStatus, string> = {
  [ShiftRequestStatus.PendingLineManagerApproval]: "Pending Line Manager",
  [ShiftRequestStatus.PendingHRApproval]: "Pending HR",
  [ShiftRequestStatus.Approved]: "Approved",
  [ShiftRequestStatus.Rejected]: "Rejected",
};

export interface ShiftChangeRequestDto {
  id: string;
  employeeId: string;
  employeeName: string;
  requestedShiftDate: string;
  requestedShiftType: ShiftType;
  reason: string;
  comments: string | null;
  status: ShiftRequestStatus;
  createdAtUtc: string;
  lineManagerReviewedAtUtc: string | null;
  hrReviewedAtUtc: string | null;
  rejectionReason: string | null;
  isDirectReport: boolean;
}

export interface SubmitShiftChangeRequest {
  requestedShiftDate: string;
  requestedShiftType: ShiftType;
  reason: string;
  comments?: string;
}

export interface ReviewShiftChangeRequest {
  approve: boolean;
  rejectionReason?: string;
}
