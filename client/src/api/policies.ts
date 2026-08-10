import { apiClient } from "./client";
import type { CreatePolicyRequest, PolicyAcknowledgmentDto, PolicyDto } from "../types/policy";

export async function createPolicy(request: CreatePolicyRequest): Promise<PolicyDto> {
  const { data } = await apiClient.post<PolicyDto>("/policies", request);
  return data;
}

export async function getPolicies(): Promise<PolicyDto[]> {
  const { data } = await apiClient.get<PolicyDto[]>("/policies");
  return data;
}

export async function acknowledgePolicy(id: string): Promise<PolicyDto> {
  const { data } = await apiClient.post<PolicyDto>(`/policies/${id}/acknowledge`);
  return data;
}

export async function getPolicyAcknowledgments(id: string): Promise<PolicyAcknowledgmentDto[]> {
  const { data } = await apiClient.get<PolicyAcknowledgmentDto[]>(`/policies/${id}/acknowledgments`);
  return data;
}
