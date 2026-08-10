export interface AttendanceSummary {
  presentCount: number;
  activeEmployeeCount: number;
  percentPresent: number;
}

export interface DashboardSummary {
  attendance: AttendanceSummary;
  pendingLeaveCount: number;
  incidentsThisMonth: number;
  medicalCompliancePercent: number | null;
  trainingCompliancePercent: number | null;
  myAveragePerformanceScore: number | null;
}
