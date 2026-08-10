import { apiClient } from "./client";
import type {
  ReviewShiftChangeRequest,
  ShiftChangeRequestDto,
  SubmitShiftChangeRequest,
} from "../types/workShift";

export async function submitShiftChangeRequest(
  request: SubmitShiftChangeRequest,
): Promise<ShiftChangeRequestDto> {
  const { data } = await apiClient.post<ShiftChangeRequestDto>("/workshift", request);
  return data;
}

export async function getMyShiftChangeRequests(): Promise<ShiftChangeRequestDto[]> {
  const { data } = await apiClient.get<ShiftChangeRequestDto[]>("/workshift/mine");
  return data;
}

export async function getPendingLineManagerApprovals(): Promise<ShiftChangeRequestDto[]> {
  const { data } = await apiClient.get<ShiftChangeRequestDto[]>("/workshift/pending/line-manager");
  return data;
}

export async function getPendingHRApprovals(): Promise<ShiftChangeRequestDto[]> {
  const { data } = await apiClient.get<ShiftChangeRequestDto[]>("/workshift/pending/hr");
  return data;
}

export async function lineManagerReview(
  id: string,
  review: ReviewShiftChangeRequest,
): Promise<ShiftChangeRequestDto> {
  const { data } = await apiClient.post<ShiftChangeRequestDto>(
    `/workshift/${id}/line-manager-review`,
    review,
  );
  return data;
}

export async function hrReview(
  id: string,
  review: ReviewShiftChangeRequest,
): Promise<ShiftChangeRequestDto> {
  const { data } = await apiClient.post<ShiftChangeRequestDto>(`/workshift/${id}/hr-review`, review);
  return data;
}
