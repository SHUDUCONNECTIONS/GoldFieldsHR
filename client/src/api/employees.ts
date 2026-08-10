import { apiClient } from "./client";
import type {
  EmployeeSummaryDto,
  SetEmployeeActiveStatusRequest,
  SetEmployeeManagerRequest,
  SetEmployeeRoleRequest,
} from "../types/employee";

export async function getAllEmployees(): Promise<EmployeeSummaryDto[]> {
  const { data } = await apiClient.get<EmployeeSummaryDto[]>("/employees");
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
