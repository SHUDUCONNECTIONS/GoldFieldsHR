import type { ShiftRequestStatus, ShiftType } from "./workShift";

export interface RecentShiftRequest {
  id: string;
  employeeName: string;
  requestedShiftDate: string;
  requestedShiftType: ShiftType;
  status: ShiftRequestStatus;
  createdAtUtc: string;
}

export interface DashboardSummary {
  pendingLeaveCount: number;
  incidentsThisMonth: number;
  medicalCompliancePercent: number | null;
  trainingCompliancePercent: number | null;
  myKpiOverallScorePercent: number | null;
  recentShiftRequests: RecentShiftRequest[];
}
