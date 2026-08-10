import { apiClient } from "./client";
import type { MedicalExaminationDto, RecordMedicalExaminationRequest } from "../types/medical";

export async function recordMedicalExamination(
  request: RecordMedicalExaminationRequest,
): Promise<MedicalExaminationDto> {
  const { data } = await apiClient.post<MedicalExaminationDto>("/medical", request);
  return data;
}

export async function getMyMedicalExaminations(): Promise<MedicalExaminationDto[]> {
  const { data } = await apiClient.get<MedicalExaminationDto[]>("/medical/mine");
  return data;
}

export async function getAllMedicalExaminations(): Promise<MedicalExaminationDto[]> {
  const { data } = await apiClient.get<MedicalExaminationDto[]>("/medical");
  return data;
}
