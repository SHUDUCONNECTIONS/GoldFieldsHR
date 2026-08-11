import { apiClient } from "./client";
import type { AnnouncementDto, CreateAnnouncementRequest } from "../types/announcement";

export async function createAnnouncement(request: CreateAnnouncementRequest): Promise<AnnouncementDto> {
  const { data } = await apiClient.post<AnnouncementDto>("/announcements", request);
  return data;
}

export async function getAnnouncements(): Promise<AnnouncementDto[]> {
  const { data } = await apiClient.get<AnnouncementDto[]>("/announcements");
  return data;
}
