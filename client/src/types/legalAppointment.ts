export const LegalAppointmentType = {
  MineManager2_1: 0,
  Engineer2_6_1: 1,
  HealthAndSafetyOfficer2_13_1: 2,
  VentilationOfficer2_17_1: 3,
  CompetentPerson3_1a: 4,
  ShiftSupervisor12_1: 5,
  Other: 6,
} as const;
export type LegalAppointmentType = (typeof LegalAppointmentType)[keyof typeof LegalAppointmentType];

export const LegalAppointmentTypeLabels: Record<LegalAppointmentType, string> = {
  [LegalAppointmentType.MineManager2_1]: "2.1 – Mine Manager",
  [LegalAppointmentType.Engineer2_6_1]: "2.6.1 – Engineer",
  [LegalAppointmentType.HealthAndSafetyOfficer2_13_1]: "2.13.1 – Health & Safety Officer",
  [LegalAppointmentType.VentilationOfficer2_17_1]: "2.17.1 – Ventilation Officer",
  [LegalAppointmentType.CompetentPerson3_1a]: "3.1(a) – Competent Person",
  [LegalAppointmentType.ShiftSupervisor12_1]: "12.1 – Shift Supervisor",
  [LegalAppointmentType.Other]: "Other",
};

export const LegalAppointmentStatus = {
  Pending: 0,
  Active: 1,
  Rejected: 2,
  Revoked: 3,
} as const;
export type LegalAppointmentStatus = (typeof LegalAppointmentStatus)[keyof typeof LegalAppointmentStatus];

export const LegalAppointmentStatusLabels: Record<LegalAppointmentStatus, string> = {
  [LegalAppointmentStatus.Pending]: "Pending",
  [LegalAppointmentStatus.Active]: "Active",
  [LegalAppointmentStatus.Rejected]: "Rejected",
  [LegalAppointmentStatus.Revoked]: "Revoked",
};

export interface LegalAppointmentDto {
  id: string;
  employeeId: string;
  employeeName: string;
  appointmentType: LegalAppointmentType;
  appointedBy: string;
  description: string;
  validFrom: string;
  validTo: string;
  status: LegalAppointmentStatus;
  createdAtUtc: string;
  reviewedAtUtc: string | null;
  rejectionReason: string | null;
  revokedAtUtc: string | null;
  revokedNotes: string | null;
}

export interface SubmitLegalAppointmentRequest {
  appointmentType: LegalAppointmentType;
  appointedBy: string;
  description: string;
  validFrom: string;
  validTo: string;
}

export interface ReviewLegalAppointmentRequest {
  approve: boolean;
  rejectionReason?: string;
}

export interface RevokeLegalAppointmentRequest {
  revokedNotes?: string;
}
