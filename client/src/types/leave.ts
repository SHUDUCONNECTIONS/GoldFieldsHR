export const LeaveType = {
  Annual: 0,
  Sick: 1,
  FamilyResponsibility: 2,
  Unpaid: 3,
  InjuryOnDuty: 4,
  Maternity: 5,
} as const;
export type LeaveType = (typeof LeaveType)[keyof typeof LeaveType];

export const LeaveTypeLabels: Record<LeaveType, string> = {
  [LeaveType.Annual]: "Annual Leave",
  [LeaveType.Sick]: "Sick Leave",
  [LeaveType.FamilyResponsibility]: "Family Responsibility Leave",
  [LeaveType.Unpaid]: "Unpaid Leave",
  [LeaveType.InjuryOnDuty]: "Injury on Duty",
  [LeaveType.Maternity]: "Maternity Leave",
};

// Sick Leave and Injury on Duty require a medical certificate on the paper form.
export const LEAVE_TYPES_REQUIRING_CERTIFICATE: readonly LeaveType[] = [LeaveType.Sick, LeaveType.InjuryOnDuty];

export const LeaveRequestStatus = {
  Pending: 0,
  Approved: 1,
  Rejected: 2,
} as const;
export type LeaveRequestStatus = (typeof LeaveRequestStatus)[keyof typeof LeaveRequestStatus];

export const LeaveRequestStatusLabels: Record<LeaveRequestStatus, string> = {
  [LeaveRequestStatus.Pending]: "Pending",
  [LeaveRequestStatus.Approved]: "Approved",
  [LeaveRequestStatus.Rejected]: "Rejected",
};

export interface LeaveRequestDto {
  id: string;
  employeeId: string;
  employeeName: string;
  leaveType: LeaveType;
  startDate: string;
  endDate: string;
  daysRequested: number;
  reason: string;
  contactNumber: string;
  status: LeaveRequestStatus;
  createdAtUtc: string;
  reviewedAtUtc: string | null;
  rejectionReason: string | null;
  isDirectReport: boolean;
}

export interface SubmitLeaveRequest {
  leaveType: LeaveType;
  startDate: string;
  endDate: string;
  reason: string;
  contactNumber: string;
}

export interface ReviewLeaveRequest {
  approve: boolean;
  rejectionReason?: string;
}
