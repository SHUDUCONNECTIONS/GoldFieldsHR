import { apiClient } from "./client";
import type {
  EmployeeDirectoryQuery,
  EmployeeSummaryDto,
  PagedResult,
  SetEmployeeActiveStatusRequest,
  SetEmployeeManagerRequest,
  SetEmployeeRoleRequest,
} from "../types/employee";

export async function getEmployees(query: EmployeeDirectoryQuery = {}): Promise<PagedResult<EmployeeSummaryDto>> {
  const { data } = await apiClient.get<PagedResult<EmployeeSummaryDto>>("/employees", { params: query });
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
