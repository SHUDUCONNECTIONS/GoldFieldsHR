import { apiClient } from "./client";
import type {
  ReviewTimesheetCorrectionRequest,
  SubmitTimesheetCorrectionRequest,
  TimesheetCorrectionDto,
  TimesheetEntry,
} from "../types/timesheet";

export async function clockIn(): Promise<TimesheetEntry> {
  const { data } = await apiClient.post<TimesheetEntry>("/timesheet/clock-in");
  return data;
}

export async function clockOut(): Promise<TimesheetEntry> {
  const { data } = await apiClient.post<TimesheetEntry>("/timesheet/clock-out");
  return data;
}

export async function getToday(): Promise<TimesheetEntry | null> {
  const { data } = await apiClient.get<TimesheetEntry | null>("/timesheet/today");
  return data;
}

export async function getHistory(): Promise<TimesheetEntry[]> {
  const { data } = await apiClient.get<TimesheetEntry[]>("/timesheet");
  return data;
}

export async function submitTimesheetCorrection(
  request: SubmitTimesheetCorrectionRequest,
): Promise<TimesheetCorrectionDto> {
  const { data } = await apiClient.post<TimesheetCorrectionDto>("/timesheet/corrections", request);
  return data;
}

export async function getMyTimesheetCorrections(): Promise<TimesheetCorrectionDto[]> {
  const { data } = await apiClient.get<TimesheetCorrectionDto[]>("/timesheet/corrections/mine");
  return data;
}

export async function getPendingTimesheetCorrections(): Promise<TimesheetCorrectionDto[]> {
  const { data } = await apiClient.get<TimesheetCorrectionDto[]>("/timesheet/corrections/pending");
  return data;
}

export async function reviewTimesheetCorrection(
  id: string,
  review: ReviewTimesheetCorrectionRequest,
): Promise<TimesheetCorrectionDto> {
  const { data } = await apiClient.post<TimesheetCorrectionDto>(`/timesheet/corrections/${id}/review`, review);
  return data;
}
