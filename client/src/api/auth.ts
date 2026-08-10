import { apiClient } from "./client";
import type { AuthResponse, LoginRequest, RegisterRequest, Site } from "../types/auth";

export async function login(request: LoginRequest): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>("/auth/login", request);
  return data;
}

export async function register(request: RegisterRequest): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>("/auth/register", request);
  return data;
}

export async function getSites(): Promise<Site[]> {
  const { data } = await apiClient.get<Site[]>("/sites");
  return data;
}

export interface ForgotPasswordResponse {
  message: string;
  devResetToken?: string;
}

export async function forgotPassword(email: string): Promise<ForgotPasswordResponse> {
  const { data } = await apiClient.post<ForgotPasswordResponse>("/auth/forgot-password", { email });
  return data;
}

export async function resetPassword(email: string, token: string, newPassword: string): Promise<void> {
  await apiClient.post("/auth/reset-password", { email, token, newPassword });
}
