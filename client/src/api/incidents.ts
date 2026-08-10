import { apiClient } from "./client";
import type {
  IncidentReportDto,
  SubmitIncidentReportRequest,
  UpdateIncidentStatusRequest,
} from "../types/incident";

export async function submitIncidentReport(request: SubmitIncidentReportRequest): Promise<IncidentReportDto> {
  const { data } = await apiClient.post<IncidentReportDto>("/incidents", request);
  return data;
}

export async function getMyIncidentReports(): Promise<IncidentReportDto[]> {
  const { data } = await apiClient.get<IncidentReportDto[]>("/incidents/mine");
  return data;
}

export async function getAllIncidentReports(): Promise<IncidentReportDto[]> {
  const { data } = await apiClient.get<IncidentReportDto[]>("/incidents");
  return data;
}

export async function updateIncidentStatus(
  id: string,
  request: UpdateIncidentStatusRequest,
): Promise<IncidentReportDto> {
  const { data } = await apiClient.post<IncidentReportDto>(`/incidents/${id}/status`, request);
  return data;
}
