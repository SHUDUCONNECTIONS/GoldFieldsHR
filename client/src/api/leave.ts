import { apiClient } from "./client";
import type { LeaveRequestDto, ReviewLeaveRequest, SubmitLeaveRequest } from "../types/leave";

export async function submitLeaveRequest(request: SubmitLeaveRequest): Promise<LeaveRequestDto> {
  const { data } = await apiClient.post<LeaveRequestDto>("/leave", request);
  return data;
}

export async function getMyLeaveRequests(): Promise<LeaveRequestDto[]> {
  const { data } = await apiClient.get<LeaveRequestDto[]>("/leave/mine");
  return data;
}

export async function getPendingLineManagerApprovals(): Promise<LeaveRequestDto[]> {
  const { data } = await apiClient.get<LeaveRequestDto[]>("/leave/pending/line-manager");
  return data;
}

export async function getPendingHRApprovals(): Promise<LeaveRequestDto[]> {
  const { data } = await apiClient.get<LeaveRequestDto[]>("/leave/pending/hr");
  return data;
}

export async function lineManagerReview(id: string, review: ReviewLeaveRequest): Promise<LeaveRequestDto> {
  const { data } = await apiClient.post<LeaveRequestDto>(`/leave/${id}/line-manager-review`, review);
  return data;
}

export async function hrReview(id: string, review: ReviewLeaveRequest): Promise<LeaveRequestDto> {
  const { data } = await apiClient.post<LeaveRequestDto>(`/leave/${id}/hr-review`, review);
  return data;
}

export async function downloadSignedLeaveDocument(id: string, fileName: string): Promise<void> {
  const response = await apiClient.get(`/leave/${id}/signed-document`, { responseType: "blob" });
  const url = URL.createObjectURL(response.data as Blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}
