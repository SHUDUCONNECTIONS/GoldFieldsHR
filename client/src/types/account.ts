import type { EmployeeRole } from "./auth";

export interface ProfileDto {
  employeeId: string;
  fullName: string;
  email: string;
  employeeNumber: string;
  jobTitle: string;
  role: EmployeeRole;
  siteName: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface SignatureDto {
  hasSignature: boolean;
  signaturePngBase64: string | null;
  updatedAtUtc: string | null;
}

export interface SetSignatureRequest {
  signaturePngBase64: string;
}
