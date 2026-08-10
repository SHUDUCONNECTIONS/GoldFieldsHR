export const FitnessStatus = {
  Fit: 0,
  FitWithRestrictions: 1,
  Unfit: 2,
  Pending: 3,
} as const;
export type FitnessStatus = (typeof FitnessStatus)[keyof typeof FitnessStatus];

export const FitnessStatusLabels: Record<FitnessStatus, string> = {
  [FitnessStatus.Fit]: "Fit",
  [FitnessStatus.FitWithRestrictions]: "Fit with restrictions",
  [FitnessStatus.Unfit]: "Unfit",
  [FitnessStatus.Pending]: "Pending",
};

export interface MedicalExaminationDto {
  id: string;
  employeeId: string;
  employeeName: string;
  examDate: string;
  expiryDate: string;
  status: FitnessStatus;
  restrictions: string | null;
  notes: string | null;
  examinedByName: string;
}

export interface RecordMedicalExaminationRequest {
  employeeNumber: string;
  examDate: string;
  expiryDate: string;
  status: FitnessStatus;
  restrictions?: string;
  notes?: string;
}
