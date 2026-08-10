import { Badge, type BadgeTone } from "./Badge";
import { CertificateStatus, CertificateStatusLabels } from "../types/certificate";

const toneByStatus: Record<CertificateStatus, BadgeTone> = {
  [CertificateStatus.Valid]: "emerald",
  [CertificateStatus.DueSoon]: "amber",
  [CertificateStatus.Expired]: "red",
};

export function CertificateStatusBadge({ status }: { status: CertificateStatus }) {
  return <Badge label={CertificateStatusLabels[status]} tone={toneByStatus[status]} />;
}
