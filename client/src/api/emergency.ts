import { apiClient } from "./client";
import type {
  EmergencyAlertDto,
  ResolveEmergencyAlertRequest,
  TriggerEmergencyAlertRequest,
} from "../types/emergency";

export async function triggerEmergencyAlert(request: TriggerEmergencyAlertRequest): Promise<EmergencyAlertDto> {
  const { data } = await apiClient.post<EmergencyAlertDto>("/emergency", request);
  return data;
}

export async function getMyEmergencyAlerts(): Promise<EmergencyAlertDto[]> {
  const { data } = await apiClient.get<EmergencyAlertDto[]>("/emergency/mine");
  return data;
}

export async function getActiveEmergencyAlerts(): Promise<EmergencyAlertDto[]> {
  const { data } = await apiClient.get<EmergencyAlertDto[]>("/emergency/active");
  return data;
}

export async function resolveEmergencyAlert(
  id: string,
  request: ResolveEmergencyAlertRequest,
): Promise<EmergencyAlertDto> {
  const { data } = await apiClient.post<EmergencyAlertDto>(`/emergency/${id}/resolve`, request);
  return data;
}
