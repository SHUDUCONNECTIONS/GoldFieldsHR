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

export async function getPendingLeaveApprovals(): Promise<LeaveRequestDto[]> {
  const { data } = await apiClient.get<LeaveRequestDto[]>("/leave/pending");
  return data;
}

export async function reviewLeaveRequest(
  id: string,
  review: ReviewLeaveRequest,
): Promise<LeaveRequestDto> {
  const { data } = await apiClient.post<LeaveRequestDto>(`/leave/${id}/review`, review);
  return data;
}
