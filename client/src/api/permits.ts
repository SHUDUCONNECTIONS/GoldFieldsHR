import { apiClient } from "./client";
import type {
  ClosePermitRequest,
  ReviewPermitRequest,
  SubmitPermitRequest,
  WorkPermitDto,
} from "../types/permit";

export async function submitPermitRequest(request: SubmitPermitRequest): Promise<WorkPermitDto> {
  const { data } = await apiClient.post<WorkPermitDto>("/permits", request);
  return data;
}

export async function getMyPermits(): Promise<WorkPermitDto[]> {
  const { data } = await apiClient.get<WorkPermitDto[]>("/permits/mine");
  return data;
}

export async function getPendingPermitApprovals(): Promise<WorkPermitDto[]> {
  const { data } = await apiClient.get<WorkPermitDto[]>("/permits/pending");
  return data;
}

export async function getOpenPermits(): Promise<WorkPermitDto[]> {
  const { data } = await apiClient.get<WorkPermitDto[]>("/permits/open");
  return data;
}

export async function reviewPermit(id: string, review: ReviewPermitRequest): Promise<WorkPermitDto> {
  const { data } = await apiClient.post<WorkPermitDto>(`/permits/${id}/review`, review);
  return data;
}

export async function closePermit(id: string, request: ClosePermitRequest): Promise<WorkPermitDto> {
  const { data } = await apiClient.post<WorkPermitDto>(`/permits/${id}/close`, request);
  return data;
}
