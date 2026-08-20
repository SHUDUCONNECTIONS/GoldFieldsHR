import { apiClient } from "./client";
import type { EmployeePerformanceDto, MyPerformanceDto, PerformanceRange } from "../types/performance";

export async function getMyPerformance(range: PerformanceRange): Promise<MyPerformanceDto> {
  const { data } = await apiClient.get<MyPerformanceDto>("/performance/mine", { params: { range } });
  return data;
}

export async function getOrgPerformance(range: PerformanceRange, siteId?: string): Promise<EmployeePerformanceDto[]> {
  const { data } = await apiClient.get<EmployeePerformanceDto[]>("/performance/org", {
    params: siteId ? { range, siteId } : { range },
  });
  return data;
}
