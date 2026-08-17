import { apiClient } from "./client";
import type { CreateScheduleDocumentRequest, PostedScheduleDocumentDto } from "../types/scheduleDocument";

export async function createScheduleDocument(
  request: CreateScheduleDocumentRequest,
): Promise<PostedScheduleDocumentDto> {
  const { data } = await apiClient.post<PostedScheduleDocumentDto>("/schedule-documents", request);
  return data;
}

export async function getScheduleDocuments(): Promise<PostedScheduleDocumentDto[]> {
  const { data } = await apiClient.get<PostedScheduleDocumentDto[]>("/schedule-documents");
  return data;
}

export async function deleteScheduleDocument(id: string): Promise<void> {
  await apiClient.delete(`/schedule-documents/${id}`);
}
