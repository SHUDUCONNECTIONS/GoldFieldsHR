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

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface EmployeeDirectoryQuery {
  search?: string;
  role?: EmployeeRole;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
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
