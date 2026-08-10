import type { EmployeeRole } from "./auth";

export interface EmployeeSummaryDto {
  id: string;
  employeeNumber: string;
  fullName: string;
  email: string;
  jobTitle: string;
  role: EmployeeRole;
  siteName: string;
  isActive: boolean;
  createdAtUtc: string;
  managerId: string | null;
  managerName: string | null;
}

export interface SetEmployeeActiveStatusRequest {
  isActive: boolean;
}

export interface SetEmployeeManagerRequest {
  managerEmployeeNumber?: string;
}

export interface SetEmployeeRoleRequest {
  role: EmployeeRole;
}
