import { apiClient } from "./client";
import type { ChangePasswordRequest, ProfileDto, SetSignatureRequest, SignatureDto } from "../types/account";

export async function getProfile(): Promise<ProfileDto> {
  const { data } = await apiClient.get<ProfileDto>("/account/profile");
  return data;
}

export async function changePassword(request: ChangePasswordRequest): Promise<void> {
  await apiClient.post("/account/change-password", request);
}

export async function getSignature(): Promise<SignatureDto> {
  const { data } = await apiClient.get<SignatureDto>("/account/signature");
  return data;
}

export async function setSignature(request: SetSignatureRequest): Promise<SignatureDto> {
  const { data } = await apiClient.put<SignatureDto>("/account/signature", request);
  return data;
}
