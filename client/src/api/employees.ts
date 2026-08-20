import { apiClient } from "./client";
import type {
  EmployeeDirectoryQuery,
  EmployeeSummaryDto,
  PagedResult,
  SetEmployeeActiveStatusRequest,
  SetEmployeeManagerRequest,
  SetEmployeeRoleRequest,
} from "../types/employee";
import type { EmployeeLiteDto } from "../types/board";

export async function getEmployees(query: EmployeeDirectoryQuery = {}): Promise<PagedResult<EmployeeSummaryDto>> {
  const { data } = await apiClient.get<PagedResult<EmployeeSummaryDto>>("/employees", { params: query });
  return data;
}

/** Lightweight active-employee list any authenticated user can call, for member/assignee pickers. */
export async function getEmployeeDirectoryLite(): Promise<EmployeeLiteDto[]> {
  const { data } = await apiClient.get<EmployeeLiteDto[]>("/employees/directory-lite");
  return data;
}

export async function setEmployeeActiveStatus(
  id: string,
  request: SetEmployeeActiveStatusRequest,
): Promise<EmployeeSummaryDto> {
  const { data } = await apiClient.patch<EmployeeSummaryDto>(`/employees/${id}/status`, request);
  return data;
}

export async function setEmployeeManager(
  id: string,
  request: SetEmployeeManagerRequest,
): Promise<EmployeeSummaryDto> {
  const { data } = await apiClient.patch<EmployeeSummaryDto>(`/employees/${id}/manager`, request);
  return data;
}

export async function setEmployeeRole(id: string, request: SetEmployeeRoleRequest): Promise<EmployeeSummaryDto> {
  const { data } = await apiClient.patch<EmployeeSummaryDto>(`/employees/${id}/role`, request);
  return data;
}

export async function dismissRequestedRole(id: string): Promise<EmployeeSummaryDto> {
  const { data } = await apiClient.patch<EmployeeSummaryDto>(`/employees/${id}/requested-role/dismiss`);
  return data;
}
