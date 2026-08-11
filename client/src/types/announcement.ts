export interface AnnouncementDto {
  id: string;
  title: string;
  body: string;
  postedByName: string;
  createdAtUtc: string;
}

export interface CreateAnnouncementRequest {
  title: string;
  body: string;
}
