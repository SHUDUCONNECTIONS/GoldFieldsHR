import type { EmployeeRole } from "./auth";

export interface RoleHeadcountDto {
  role: EmployeeRole;
  count: number;
}

export interface ReportsSummaryDto {
  totalEmployees: number;
  activeEmployees: number;
  headcountByRole: RoleHeadcountDto[];
  pendingLeaveRequests: number;
  validCertificates: number;
  dueSoonCertificates: number;
  expiredCertificates: number;
  pendingPpeRequests: number;
  ppeAwaitingIssue: number;
  pendingLegalAppointments: number;
  activeLegalAppointments: number;
}
