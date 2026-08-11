import type { ShiftRequestStatus, ShiftType } from "./workShift";

export interface AttendanceSummary {
  presentCount: number;
  activeEmployeeCount: number;
  percentPresent: number;
}

export interface RecentShiftRequest {
  id: string;
  employeeName: string;
  requestedShiftDate: string;
  requestedShiftType: ShiftType;
  status: ShiftRequestStatus;
  createdAtUtc: string;
}

export interface DashboardSummary {
  attendance: AttendanceSummary;
  pendingLeaveCount: number;
  incidentsThisMonth: number;
  medicalCompliancePercent: number | null;
  trainingCompliancePercent: number | null;
  myAveragePerformanceScore: number | null;
  recentShiftRequests: RecentShiftRequest[];
}
