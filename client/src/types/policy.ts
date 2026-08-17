export interface PolicyDto {
  id: string;
  title: string;
  content: string;
  publishedByName: string;
  publishedAtUtc: string;
  acknowledgedByMe: boolean;
  acknowledgedAtUtc: string | null;
  acknowledgmentCount: number;
}

export interface CreatePolicyRequest {
  title: string;
  content: string;
}

export interface AcknowledgePolicyRequest {
  signaturePngBase64?: string;
}

export interface PolicyAcknowledgmentDto {
  employeeId: string;
  employeeName: string;
  acknowledgedAtUtc: string;
}
