export interface PostedScheduleDocumentDto {
  id: string;
  title: string;
  postedByEmployeeId: string;
  postedByName: string;
  postedAtUtc: string;
}

export interface CreateScheduleDocumentRequest {
  title: string;
}
