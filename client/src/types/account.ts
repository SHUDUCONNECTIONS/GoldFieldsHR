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
