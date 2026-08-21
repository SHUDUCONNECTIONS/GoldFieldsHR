import { apiClient } from "./client";
import type {
  CompletedBoardDto,
  EmployeePerformanceDto,
  MyPerformanceDto,
  OrgPerformanceSummaryDto,
  PerformanceRange,
} from "../types/performance";

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

export async function getOrgPerformanceSummary(siteId?: string): Promise<OrgPerformanceSummaryDto> {
  const { data } = await apiClient.get<OrgPerformanceSummaryDto>("/performance/org/summary", {
    params: siteId ? { siteId } : undefined,
  });
  return data;
}

export async function getCompletedBoards(siteId?: string): Promise<CompletedBoardDto[]> {
  const { data } = await apiClient.get<CompletedBoardDto[]>("/performance/completed-boards", {
    params: siteId ? { siteId } : undefined,
  });
  return data;
}

export async function downloadEmployeePerformancePdf(employeeId: string, employeeName: string): Promise<void> {
  const response = await apiClient.get(`/performance/employee/${employeeId}/pdf`, { responseType: "blob" });
  const url = URL.createObjectURL(response.data as Blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = `performance-${employeeName}.pdf`;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}
