import { apiClient } from "./client";
import type { PpeRequestDto, ReviewPpeRequest, SubmitPpeRequest } from "../types/ppe";

export async function submitPpeRequest(request: SubmitPpeRequest): Promise<PpeRequestDto> {
  const { data } = await apiClient.post<PpeRequestDto>("/ppe", request);
  return data;
}

export async function getMyPpeRequests(): Promise<PpeRequestDto[]> {
  const { data } = await apiClient.get<PpeRequestDto[]>("/ppe/mine");
  return data;
}

export async function getPendingPpeApprovals(): Promise<PpeRequestDto[]> {
  const { data } = await apiClient.get<PpeRequestDto[]>("/ppe/pending");
  return data;
}

export async function getPpeAwaitingIssue(): Promise<PpeRequestDto[]> {
  const { data } = await apiClient.get<PpeRequestDto[]>("/ppe/awaiting-issue");
  return data;
}

export async function reviewPpeRequest(id: string, review: ReviewPpeRequest): Promise<PpeRequestDto> {
  const { data } = await apiClient.post<PpeRequestDto>(`/ppe/${id}/review`, review);
  return data;
}

export async function issuePpeRequest(id: string): Promise<PpeRequestDto> {
  const { data } = await apiClient.post<PpeRequestDto>(`/ppe/${id}/issue`, {});
  return data;
}
