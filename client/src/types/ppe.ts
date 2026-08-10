export const PpeItemType = {
  Helmet: 0,
  Gloves: 1,
  SafetyBoots: 2,
  HiVisVest: 3,
  EarProtection: 4,
  SafetyGoggles: 5,
  Respirator: 6,
  Coveralls: 7,
  Other: 8,
} as const;
export type PpeItemType = (typeof PpeItemType)[keyof typeof PpeItemType];

export const PpeItemTypeLabels: Record<PpeItemType, string> = {
  [PpeItemType.Helmet]: "Helmet",
  [PpeItemType.Gloves]: "Gloves",
  [PpeItemType.SafetyBoots]: "Safety Boots",
  [PpeItemType.HiVisVest]: "Hi-Vis Vest",
  [PpeItemType.EarProtection]: "Ear Protection",
  [PpeItemType.SafetyGoggles]: "Safety Goggles",
  [PpeItemType.Respirator]: "Respirator",
  [PpeItemType.Coveralls]: "Coveralls",
  [PpeItemType.Other]: "Other",
};

export const PpeRequestStatus = {
  Pending: 0,
  Approved: 1,
  Rejected: 2,
  Issued: 3,
} as const;
export type PpeRequestStatus = (typeof PpeRequestStatus)[keyof typeof PpeRequestStatus];

export const PpeRequestStatusLabels: Record<PpeRequestStatus, string> = {
  [PpeRequestStatus.Pending]: "Pending",
  [PpeRequestStatus.Approved]: "Approved",
  [PpeRequestStatus.Rejected]: "Rejected",
  [PpeRequestStatus.Issued]: "Issued",
};

export interface PpeRequestDto {
  id: string;
  employeeId: string;
  employeeName: string;
  itemType: PpeItemType;
  size: string | null;
  quantity: number;
  reason: string;
  status: PpeRequestStatus;
  createdAtUtc: string;
  reviewedAtUtc: string | null;
  rejectionReason: string | null;
  issuedAtUtc: string | null;
}

export interface SubmitPpeRequest {
  itemType: PpeItemType;
  size?: string;
  quantity: number;
  reason: string;
}

export interface ReviewPpeRequest {
  approve: boolean;
  rejectionReason?: string;
}
