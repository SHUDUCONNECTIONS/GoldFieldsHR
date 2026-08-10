import { apiClient } from "./client";
import type { CertificateDto, IssueCertificateRequest } from "../types/certificate";

export async function issueCertificate(request: IssueCertificateRequest): Promise<CertificateDto> {
  const { data } = await apiClient.post<CertificateDto>("/certificates", request);
  return data;
}

export async function getMyCertificates(): Promise<CertificateDto[]> {
  const { data } = await apiClient.get<CertificateDto[]>("/certificates/mine");
  return data;
}

export async function getAllCertificates(): Promise<CertificateDto[]> {
  const { data } = await apiClient.get<CertificateDto[]>("/certificates");
  return data;
}
