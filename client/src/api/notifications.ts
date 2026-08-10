import { apiClient } from "./client";
import type { NotificationDto } from "../types/notification";

export async function getMyNotifications(): Promise<NotificationDto[]> {
  const { data } = await apiClient.get<NotificationDto[]>("/notifications");
  return data;
}

export async function getUnreadNotificationCount(): Promise<number> {
  const { data } = await apiClient.get<{ count: number }>("/notifications/unread-count");
  return data.count;
}

export async function markNotificationAsRead(id: string): Promise<void> {
  await apiClient.post(`/notifications/${id}/read`);
}

export async function markAllNotificationsAsRead(): Promise<void> {
  await apiClient.post("/notifications/read-all");
}
