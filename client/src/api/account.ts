import { apiClient } from "./client";
import type { ChangePasswordRequest, ProfileDto } from "../types/account";

export async function getProfile(): Promise<ProfileDto> {
  const { data } = await apiClient.get<ProfileDto>("/account/profile");
  return data;
}

export async function changePassword(request: ChangePasswordRequest): Promise<void> {
  await apiClient.post("/account/change-password", request);
}
