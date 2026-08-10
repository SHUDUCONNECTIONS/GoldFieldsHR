import { apiClient } from "./client";
import type { ReportsSummaryDto } from "../types/reports";

export async function getReportsSummary(): Promise<ReportsSummaryDto> {
  const { data } = await apiClient.get<ReportsSummaryDto>("/reports/summary");
  return data;
}
