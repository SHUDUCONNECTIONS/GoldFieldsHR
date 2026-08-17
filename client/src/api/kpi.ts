import { apiClient } from "./client";
import type {
  CreateKpiAppraisalRequest,
  KpiAppraisalDetailDto,
  KpiAppraisalSummaryDto,
  KpiTemplateSummaryDto,
  SetItemFlagsRequest,
  SignKpiAppraisalRequest,
  SubmitCheckpointScoresRequest,
} from "../types/kpi";

export async function getKpiTemplates(): Promise<KpiTemplateSummaryDto[]> {
  const { data } = await apiClient.get<KpiTemplateSummaryDto[]>("/kpi/templates");
  return data;
}

export async function createKpiAppraisal(request: CreateKpiAppraisalRequest): Promise<KpiAppraisalDetailDto> {
  const { data } = await apiClient.post<KpiAppraisalDetailDto>("/kpi/appraisals", request);
  return data;
}

export async function getMyKpiAppraisals(): Promise<KpiAppraisalSummaryDto[]> {
  const { data } = await apiClient.get<KpiAppraisalSummaryDto[]>("/kpi/appraisals/mine");
  return data;
}

export async function getManagedKpiAppraisals(): Promise<KpiAppraisalSummaryDto[]> {
  const { data } = await apiClient.get<KpiAppraisalSummaryDto[]>("/kpi/appraisals/managed");
  return data;
}

export async function getAllKpiAppraisals(): Promise<KpiAppraisalSummaryDto[]> {
  const { data } = await apiClient.get<KpiAppraisalSummaryDto[]>("/kpi/appraisals");
  return data;
}

export async function getKpiAppraisalsPendingMySignOff(): Promise<KpiAppraisalSummaryDto[]> {
  const { data } = await apiClient.get<KpiAppraisalSummaryDto[]>("/kpi/appraisals/pending-signoff");
  return data;
}

export async function getKpiAppraisalById(id: string): Promise<KpiAppraisalDetailDto> {
  const { data } = await apiClient.get<KpiAppraisalDetailDto>(`/kpi/appraisals/${id}`);
  return data;
}

export async function submitKpiCheckpointScores(
  id: string,
  request: SubmitCheckpointScoresRequest,
): Promise<KpiAppraisalDetailDto> {
  const { data } = await apiClient.post<KpiAppraisalDetailDto>(`/kpi/appraisals/${id}/scores`, request);
  return data;
}

export async function setKpiItemFlags(id: string, request: SetItemFlagsRequest): Promise<KpiAppraisalDetailDto> {
  const { data } = await apiClient.post<KpiAppraisalDetailDto>(`/kpi/appraisals/${id}/item-flags`, request);
  return data;
}

export async function signKpiAppraisalAsBlastingOfficer(
  id: string,
  request: SignKpiAppraisalRequest,
): Promise<KpiAppraisalDetailDto> {
  const { data } = await apiClient.post<KpiAppraisalDetailDto>(`/kpi/appraisals/${id}/sign/blasting-officer`, request);
  return data;
}

export async function signKpiAppraisalAsBlastingEngineer(
  id: string,
  request: SignKpiAppraisalRequest,
): Promise<KpiAppraisalDetailDto> {
  const { data } = await apiClient.post<KpiAppraisalDetailDto>(`/kpi/appraisals/${id}/sign/blasting-engineer`, request);
  return data;
}

export async function downloadKpiAppraisalPdf(id: string, fileName: string): Promise<void> {
  const response = await apiClient.get(`/kpi/appraisals/${id}/pdf`, { responseType: "blob" });
  const url = URL.createObjectURL(response.data as Blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}
