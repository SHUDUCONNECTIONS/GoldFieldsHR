export const PermitType = {
  HotWork: 0,
  ConfinedSpace: 1,
  WorkingAtHeight: 2,
  Excavation: 3,
  ElectricalIsolation: 4,
  LiftingOperation: 5,
  Other: 6,
} as const;
export type PermitType = (typeof PermitType)[keyof typeof PermitType];

export const PermitTypeLabels: Record<PermitType, string> = {
  [PermitType.HotWork]: "Hot Work",
  [PermitType.ConfinedSpace]: "Confined Space",
  [PermitType.WorkingAtHeight]: "Working at Height",
  [PermitType.Excavation]: "Excavation",
  [PermitType.ElectricalIsolation]: "Electrical Isolation",
  [PermitType.LiftingOperation]: "Lifting Operation",
  [PermitType.Other]: "Other",
};

export const PermitStatus = {
  Pending: 0,
  Approved: 1,
  Rejected: 2,
  Closed: 3,
} as const;
export type PermitStatus = (typeof PermitStatus)[keyof typeof PermitStatus];

export const PermitStatusLabels: Record<PermitStatus, string> = {
  [PermitStatus.Pending]: "Pending",
  [PermitStatus.Approved]: "Approved",
  [PermitStatus.Rejected]: "Rejected",
  [PermitStatus.Closed]: "Closed",
};

export interface WorkPermitDto {
  id: string;
  employeeId: string;
  employeeName: string;
  permitType: PermitType;
  location: string;
  description: string;
  validFrom: string;
  validTo: string;
  status: PermitStatus;
  createdAtUtc: string;
  reviewedAtUtc: string | null;
  rejectionReason: string | null;
  closedAtUtc: string | null;
  closedNotes: string | null;
}

export interface SubmitPermitRequest {
  permitType: PermitType;
  location: string;
  description: string;
  validFrom: string;
  validTo: string;
}

export interface ReviewPermitRequest {
  approve: boolean;
  rejectionReason?: string;
}

export interface ClosePermitRequest {
  closedNotes?: string;
}
