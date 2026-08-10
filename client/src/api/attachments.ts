import { apiClient } from "./client";
import type { AttachmentDto, AttachmentEntityType } from "../types/attachment";

export async function uploadAttachment(
  entityType: AttachmentEntityType,
  entityId: string,
  file: File,
): Promise<AttachmentDto> {
  const formData = new FormData();
  formData.append("file", file);
  const { data } = await apiClient.post<AttachmentDto>(`/attachments/${entityType}/${entityId}`, formData, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return data;
}

export async function getAttachmentsForEntity(
  entityType: AttachmentEntityType,
  entityId: string,
): Promise<AttachmentDto[]> {
  const { data } = await apiClient.get<AttachmentDto[]>(`/attachments/${entityType}/${entityId}`);
  return data;
}

export async function downloadAttachment(id: string, fileName: string): Promise<void> {
  const response = await apiClient.get(`/attachments/${id}/download`, { responseType: "blob" });
  const url = URL.createObjectURL(response.data as Blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}
