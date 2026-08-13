import { apiClient } from "./client";
import type {
  LegalAppointmentDto,
  ReviewLegalAppointmentRequest,
  RevokeLegalAppointmentRequest,
  SubmitLegalAppointmentRequest,
} from "../types/legalAppointment";

export async function submitLegalAppointment(request: SubmitLegalAppointmentRequest): Promise<LegalAppointmentDto> {
  const { data } = await apiClient.post<LegalAppointmentDto>("/legalappointments", request);
  return data;
}

export async function getMyLegalAppointments(): Promise<LegalAppointmentDto[]> {
  const { data } = await apiClient.get<LegalAppointmentDto[]>("/legalappointments/mine");
  return data;
}

export async function getPendingLegalAppointmentApprovals(): Promise<LegalAppointmentDto[]> {
  const { data } = await apiClient.get<LegalAppointmentDto[]>("/legalappointments/pending");
  return data;
}

export async function getActiveLegalAppointments(): Promise<LegalAppointmentDto[]> {
  const { data } = await apiClient.get<LegalAppointmentDto[]>("/legalappointments/active");
  return data;
}

export async function reviewLegalAppointment(id: string, review: ReviewLegalAppointmentRequest): Promise<LegalAppointmentDto> {
  const { data } = await apiClient.post<LegalAppointmentDto>(`/legalappointments/${id}/review`, review);
  return data;
}

export async function revokeLegalAppointment(id: string, request: RevokeLegalAppointmentRequest): Promise<LegalAppointmentDto> {
  const { data } = await apiClient.post<LegalAppointmentDto>(`/legalappointments/${id}/revoke`, request);
  return data;
}
