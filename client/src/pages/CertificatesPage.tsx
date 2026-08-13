import { useCallback, useEffect, useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { extractErrorMessage } from "../api/client";
import { getAllCertificates, getMyCertificates, issueCertificate } from "../api/certificates";
import { AttachmentsPanel } from "../components/AttachmentsPanel";
import { CertificateStatusBadge } from "../components/CertificateStatusBadge";
import { StepForm, type WizardStep } from "../components/StepForm";
import { formatDate } from "../lib/format";
import { AttachmentEntityType } from "../types/attachment";
import { EmployeeRole } from "../types/auth";
import type { CertificateDto } from "../types/certificate";
import { sanitizeEmployeeNumber } from "../utils/textInput";

const initialForm = {
  employeeNumber: "",
  title: "",
  issuedDate: "",
  expiryDate: "",
  notes: "",
};

export function CertificatesPage() {
  const { session } = useAuth();
  const isHR = session?.role === EmployeeRole.HR;

  const [myCertificates, setMyCertificates] = useState<CertificateDto[]>([]);
  const [allCertificates, setAllCertificates] = useState<CertificateDto[]>([]);
  const [form, setForm] = useState(initialForm);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const loadAll = useCallback(async () => {
    try {
      const requests: Promise<unknown>[] = [getMyCertificates().then(setMyCertificates)];
      if (isHR) requests.push(getAllCertificates().then(setAllCertificates));
      await Promise.all(requests);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  }, [isHR]);

  useEffect(() => {
    loadAll();
  }, [loadAll]);

  async function handleIssue() {
    setError(null);
    setIsSubmitting(true);
    try {
      await issueCertificate({ ...form, notes: form.notes || undefined });
      setForm(initialForm);
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  const steps: WizardStep[] = [
    {
      title: "Certificate",
      validate: () => (!form.employeeNumber || !form.title ? "Please fill in all fields." : null),
      content: (
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Employee number
            <input
              required
              value={form.employeeNumber}
              onChange={(e) => setForm((prev) => ({ ...prev, employeeNumber: sanitizeEmployeeNumber(e.target.value) }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Certificate title
            <input
              required
              value={form.title}
              onChange={(e) => setForm((prev) => ({ ...prev, title: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
            />
          </label>
        </div>
      ),
    },
    {
      title: "Dates & notes",
      validate: () => (!form.issuedDate || !form.expiryDate ? "Please fill in all fields." : null),
      content: (
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Issued date
            <input
              type="date"
              required
              value={form.issuedDate}
              onChange={(e) => setForm((prev) => ({ ...prev, issuedDate: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Expiry date
            <input
              type="date"
              required
              value={form.expiryDate}
              onChange={(e) => setForm((prev) => ({ ...prev, expiryDate: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700 sm:col-span-2">
            Notes (optional)
            <input
              value={form.notes}
              onChange={(e) => setForm((prev) => ({ ...prev, notes: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
            />
          </label>
        </div>
      ),
    },
  ];

  return (
    <div className="stagger-children flex flex-col gap-6">
      {isHR && (
        <>
          <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
            <h3 className="mb-4 text-sm font-semibold text-slate-900">Issue a certificate</h3>
            <StepForm
              steps={steps}
              onSubmit={handleIssue}
              submitLabel="Issue certificate"
              submittingLabel="Issuing..."
              isSubmitting={isSubmitting}
              error={error}
            />
          </div>

          <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
            <div className="border-b border-slate-200 px-6 py-4">
              <h3 className="text-sm font-semibold text-slate-900">All certificates</h3>
            </div>
            {allCertificates.length === 0 ? (
              <p className="px-6 py-8 text-center text-sm text-slate-500">No certificates issued yet.</p>
            ) : (
              <table className="w-full text-left text-sm">
                <thead>
                  <tr className="text-xs uppercase tracking-wide text-slate-500">
                    <th className="px-6 py-2 font-medium">Employee</th>
                    <th className="px-6 py-2 font-medium">Title</th>
                    <th className="px-6 py-2 font-medium">Expiry</th>
                    <th className="px-6 py-2 font-medium">Status</th>
                    <th className="px-6 py-2 font-medium">Issued by</th>
                    <th className="px-6 py-2 font-medium">Attachments</th>
                  </tr>
                </thead>
                <tbody>
                  {allCertificates.map((cert) => (
                    <tr key={cert.id} className="border-t border-slate-100">
                      <td className="px-6 py-2 text-slate-700">{cert.employeeName}</td>
                      <td className="px-6 py-2 text-slate-700">{cert.title}</td>
                      <td className="px-6 py-2 text-slate-700">{formatDate(cert.expiryDate)}</td>
                      <td className="px-6 py-2">
                        <CertificateStatusBadge status={cert.status} />
                      </td>
                      <td className="px-6 py-2 text-slate-500">{cert.issuedByName}</td>
                      <td className="px-6 py-2">
                        <AttachmentsPanel
                          entityType={AttachmentEntityType.Certificate}
                          entityId={cert.id}
                          canUpload
                          compact
                        />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </>
      )}

      <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-200 px-6 py-4">
          <h3 className="text-sm font-semibold text-slate-900">My certificates</h3>
        </div>
        {myCertificates.length === 0 ? (
          <p className="px-6 py-8 text-center text-sm text-slate-500">No certificates on record yet.</p>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="text-xs uppercase tracking-wide text-slate-500">
                <th className="px-6 py-2 font-medium">Title</th>
                <th className="px-6 py-2 font-medium">Issued</th>
                <th className="px-6 py-2 font-medium">Expiry</th>
                <th className="px-6 py-2 font-medium">Status</th>
                <th className="px-6 py-2 font-medium">Notes</th>
                <th className="px-6 py-2 font-medium">Attachments</th>
              </tr>
            </thead>
            <tbody>
              {myCertificates.map((cert) => (
                <tr key={cert.id} className="border-t border-slate-100">
                  <td className="px-6 py-2 text-slate-700">{cert.title}</td>
                  <td className="px-6 py-2 text-slate-700">{formatDate(cert.issuedDate)}</td>
                  <td className="px-6 py-2 text-slate-700">{formatDate(cert.expiryDate)}</td>
                  <td className="px-6 py-2">
                    <CertificateStatusBadge status={cert.status} />
                  </td>
                  <td className="px-6 py-2 text-slate-500">{cert.notes ?? "—"}</td>
                  <td className="px-6 py-2">
                    <AttachmentsPanel
                      entityType={AttachmentEntityType.Certificate}
                      entityId={cert.id}
                      canUpload={isHR}
                      compact
                    />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
