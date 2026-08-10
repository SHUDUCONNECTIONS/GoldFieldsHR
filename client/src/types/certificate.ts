export const CertificateStatus = {
  Valid: 0,
  DueSoon: 1,
  Expired: 2,
} as const;
export type CertificateStatus = (typeof CertificateStatus)[keyof typeof CertificateStatus];

export const CertificateStatusLabels: Record<CertificateStatus, string> = {
  [CertificateStatus.Valid]: "Valid",
  [CertificateStatus.DueSoon]: "Due soon",
  [CertificateStatus.Expired]: "Expired",
};

export interface CertificateDto {
  id: string;
  employeeId: string;
  employeeName: string;
  title: string;
  issuedDate: string;
  expiryDate: string;
  status: CertificateStatus;
  notes: string | null;
  issuedByName: string;
}

export interface IssueCertificateRequest {
  employeeNumber: string;
  title: string;
  issuedDate: string;
  expiryDate: string;
  notes?: string;
}
