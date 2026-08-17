export const KpiAppraisalStatus = {
  InProgress: "InProgress",
  PendingBlastingEngineerSignOff: "PendingBlastingEngineerSignOff",
  Finalized: "Finalized",
} as const;

export type KpiAppraisalStatus = (typeof KpiAppraisalStatus)[keyof typeof KpiAppraisalStatus];

export const KpiAppraisalStatusLabels: Record<KpiAppraisalStatus, string> = {
  [KpiAppraisalStatus.InProgress]: "In progress",
  [KpiAppraisalStatus.PendingBlastingEngineerSignOff]: "Awaiting Blasting Engineer sign-off",
  [KpiAppraisalStatus.Finalized]: "Finalized",
};

export interface KpiTemplateItemDto {
  id: string;
  description: string;
  subGroupLabel: string | null;
  displayOrder: number;
}

export interface KpiTemplateCategoryDto {
  id: string;
  name: string;
  displayOrder: number;
  items: KpiTemplateItemDto[];
}

export interface KpiTemplateSummaryDto {
  id: string;
  designation: string;
  isActive: boolean;
  categoryCount: number;
  itemCount: number;
  createdAtUtc: string;
}

export interface KpiTemplateDetailDto {
  id: string;
  designation: string;
  isActive: boolean;
  createdAtUtc: string;
  categories: KpiTemplateCategoryDto[];
}

export interface KpiAppraisalCategoryRollupDto {
  name: string;
  scorePercent: number | null;
  itemCount: number;
}

export interface KpiAppraisalSummaryDto {
  id: string;
  employeeId: string;
  employeeName: string;
  employeeNumber: string;
  designation: string;
  periodLabel: string;
  status: KpiAppraisalStatus;
  overallScorePercent: number | null;
  categories: KpiAppraisalCategoryRollupDto[];
  lastReviewedAtUtc: string | null;
  signedOffBy: string[];
  createdAtUtc: string;
}

export interface KpiAppraisalItemDto {
  id: string;
  categoryName: string;
  subGroupLabel: string | null;
  description: string;
  displayOrder: number;
  inPlace: boolean | null;
  ability: boolean | null;
  checkpoint1Score: number | null;
  checkpoint1Comment: string | null;
  checkpoint2Score: number | null;
  checkpoint2Comment: string | null;
  checkpoint3Score: number | null;
  checkpoint3Comment: string | null;
  checkpoint4Score: number | null;
  checkpoint4Comment: string | null;
  evaluation: string | null;
}

export interface KpiAppraisalDetailDto {
  id: string;
  employeeId: string;
  employeeName: string;
  employeeNumber: string;
  designation: string;
  periodLabel: string;
  inductionNumber: string | null;
  status: KpiAppraisalStatus;
  checkpoint1Date: string | null;
  checkpoint2Date: string | null;
  checkpoint3Date: string | null;
  checkpoint4Date: string | null;
  blastingOfficerEmployeeId: string;
  blastingOfficerName: string;
  blastingOfficerSignedAtUtc: string | null;
  blastingEngineerEmployeeId: string;
  blastingEngineerName: string;
  blastingEngineerSignedAtUtc: string | null;
  createdAtUtc: string;
  finalizedAtUtc: string | null;
  overallScorePercent: number | null;
  categories: KpiAppraisalCategoryRollupDto[];
  items: KpiAppraisalItemDto[];
}

export interface CreateKpiAppraisalRequest {
  employeeNumber: string;
  kpiTemplateId: string;
  periodLabel: string;
  inductionNumber?: string;
  blastingOfficerEmployeeNumber: string;
  blastingEngineerEmployeeNumber: string;
  checkpoint1Date?: string;
  checkpoint2Date?: string;
  checkpoint3Date?: string;
  checkpoint4Date?: string;
}

export interface KpiItemScoreEntry {
  itemId: string;
  score: number;
  comment?: string;
}

export interface SubmitCheckpointScoresRequest {
  checkpointNumber: number;
  items: KpiItemScoreEntry[];
}

export interface KpiItemFlagEntry {
  itemId: string;
  inPlace: boolean | null;
  ability: boolean | null;
}

export interface SetItemFlagsRequest {
  items: KpiItemFlagEntry[];
}

export interface SignKpiAppraisalRequest {
  signaturePngBase64?: string;
}
