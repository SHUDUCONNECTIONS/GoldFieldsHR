export interface TimesheetEntry {
  id: string;
  clockInUtc: string;
  clockOutUtc: string | null;
  durationHours: number | null;
}

export const TimesheetCorrectionStatus = {
  Pending: 0,
  Approved: 1,
  Rejected: 2,
} as const;
export type TimesheetCorrectionStatus = (typeof TimesheetCorrectionStatus)[keyof typeof TimesheetCorrectionStatus];

export const TimesheetCorrectionStatusLabels: Record<TimesheetCorrectionStatus, string> = {
  [TimesheetCorrectionStatus.Pending]: "Pending",
  [TimesheetCorrectionStatus.Approved]: "Approved",
  [TimesheetCorrectionStatus.Rejected]: "Rejected",
};

export interface TimesheetCorrectionDto {
  id: string;
  timesheetEntryId: string;
  employeeId: string;
  employeeName: string;
  originalClockInUtc: string;
  originalClockOutUtc: string | null;
  requestedClockInUtc: string | null;
  requestedClockOutUtc: string | null;
  reason: string;
  status: TimesheetCorrectionStatus;
  createdAtUtc: string;
  reviewedAtUtc: string | null;
  rejectionReason: string | null;
  isDirectReport: boolean;
}

export interface SubmitTimesheetCorrectionRequest {
  timesheetEntryId: string;
  requestedClockInUtc?: string;
  requestedClockOutUtc?: string;
  reason: string;
}

export interface ReviewTimesheetCorrectionRequest {
  approve: boolean;
  rejectionReason?: string;
}
