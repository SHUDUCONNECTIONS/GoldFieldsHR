import type { EmployeeRole } from "./auth";
import type { IncidentSeverity } from "./incident";

export interface RoleHeadcountDto {
  role: EmployeeRole;
  count: number;
}

export interface IncidentSeverityCountDto {
  severity: IncidentSeverity;
  count: number;
}

export interface ReportsAttendanceDto {
  presentCount: number;
  activeEmployeeCount: number;
  percentPresent: number;
}

export interface ReportsSummaryDto {
  totalEmployees: number;
  activeEmployees: number;
  headcountByRole: RoleHeadcountDto[];
  attendanceToday: ReportsAttendanceDto;
  openIncidents: number;
  closedIncidents: number;
  openIncidentsBySeverity: IncidentSeverityCountDto[];
  pendingLeaveRequests: number;
  validCertificates: number;
  dueSoonCertificates: number;
  expiredCertificates: number;
  pendingPpeRequests: number;
  ppeAwaitingIssue: number;
  pendingLegalAppointments: number;
  activeLegalAppointments: number;
}
