import { apiClient } from "./client";
import type { PreShiftSafetyCheck, SubmitPreShiftCheckRequest } from "../types/safety";

export async function submitPreShiftCheck(request: SubmitPreShiftCheckRequest): Promise<PreShiftSafetyCheck> {
  const { data } = await apiClient.post<PreShiftSafetyCheck>("/safety/pre-shift-check", request);
  return data;
}

export async function getTodaysPreShiftCheck(): Promise<PreShiftSafetyCheck | null> {
  const { data } = await apiClient.get<PreShiftSafetyCheck | null>("/safety/pre-shift-check/today");
  return data;
}

export async function getPreShiftCheckHistory(): Promise<PreShiftSafetyCheck[]> {
  const { data } = await apiClient.get<PreShiftSafetyCheck[]>("/safety/pre-shift-check");
  return data;
}

export async function getTodaysHazards(): Promise<PreShiftSafetyCheck[]> {
  const { data } = await apiClient.get<PreShiftSafetyCheck[]>("/safety/hazards/today");
  return data;
}
