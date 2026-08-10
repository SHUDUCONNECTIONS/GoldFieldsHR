export interface NotificationDto {
  id: string;
  message: string;
  link: string | null;
  isRead: boolean;
  createdAtUtc: string;
}
