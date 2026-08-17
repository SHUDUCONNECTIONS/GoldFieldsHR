import { apiClient } from "./client";
import type { AcknowledgmentDto, AcknowledgmentEntityType } from "../types/acknowledgment";

export async function acknowledge(entityType: AcknowledgmentEntityType, entityId: string): Promise<AcknowledgmentDto> {
  const { data } = await apiClient.post<AcknowledgmentDto>(`/acknowledgments/${entityType}/${entityId}`);
  return data;
}

export async function getAcknowledgmentsForEntity(
  entityType: AcknowledgmentEntityType,
  entityId: string,
): Promise<AcknowledgmentDto[]> {
  const { data } = await apiClient.get<AcknowledgmentDto[]>(`/acknowledgments/${entityType}/${entityId}`);
  return data;
}
