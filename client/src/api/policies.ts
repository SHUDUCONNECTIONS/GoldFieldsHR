import { apiClient } from "./client";
import type { AcknowledgePolicyRequest, CreatePolicyRequest, PolicyAcknowledgmentDto, PolicyDto } from "../types/policy";

export async function createPolicy(request: CreatePolicyRequest): Promise<PolicyDto> {
  const { data } = await apiClient.post<PolicyDto>("/policies", request);
  return data;
}

export async function getPolicies(): Promise<PolicyDto[]> {
  const { data } = await apiClient.get<PolicyDto[]>("/policies");
  return data;
}

export async function acknowledgePolicy(id: string, request: AcknowledgePolicyRequest = {}): Promise<PolicyDto> {
  const { data } = await apiClient.post<PolicyDto>(`/policies/${id}/acknowledge`, request);
  return data;
}

export async function getPolicyAcknowledgments(id: string): Promise<PolicyAcknowledgmentDto[]> {
  const { data } = await apiClient.get<PolicyAcknowledgmentDto[]>(`/policies/${id}/acknowledgments`);
  return data;
}

export async function downloadSignedPolicyAttachment(
  policyId: string,
  employeeId: string,
  attachmentId: string,
  fileName: string,
): Promise<void> {
  const response = await apiClient.get(
    `/policies/${policyId}/acknowledgments/${employeeId}/attachments/${attachmentId}/signed`,
    { responseType: "blob" },
  );
  const url = URL.createObjectURL(response.data as Blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}
