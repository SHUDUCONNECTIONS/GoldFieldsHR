import { useCallback, useEffect, useRef, useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { uploadAttachment } from "../api/attachments";
import { extractErrorMessage } from "../api/client";
import {
  getActiveLegalAppointments,
  getMyLegalAppointments,
  getPendingLegalAppointmentApprovals,
  revokeLegalAppointment,
  reviewLegalAppointment,
  submitLegalAppointment,
} from "../api/legalAppointments";
import { AttachmentsPanel } from "../components/AttachmentsPanel";
import { LegalAppointmentApprovalQueue } from "../components/LegalAppointmentApprovalQueue";
import { LegalAppointmentRevokeQueue } from "../components/LegalAppointmentRevokeQueue";
import { LegalAppointmentStatusBadge } from "../components/LegalAppointmentStatusBadge";
import { StepForm, type WizardStep } from "../components/StepForm";
import { formatDate, formatDateTime } from "../lib/format";
import { AttachmentEntityType } from "../types/attachment";
import { EmployeeRole } from "../types/auth";
import { LegalAppointmentType, LegalAppointmentTypeLabels, type LegalAppointmentDto } from "../types/legalAppointment";

interface LegalAppointmentRequestForm {
  appointmentType: LegalAppointmentType;
  appointedBy: string;
  description: string;
  validFrom: string;
  validTo: string;
}

const initialForm: LegalAppointmentRequestForm = {
  appointmentType: LegalAppointmentType.MineManager2_1,
  appointedBy: "",
  description: "",
  validFrom: "",
  validTo: "",
};

export function LegalAppointmentsPage() {
  const { session } = useAuth();
  const isSafetyOfficer = session?.role === EmployeeRole.SafetyOfficer;
  const isHR = session?.role === EmployeeRole.HR;
  const isExecutive = session?.role === EmployeeRole.Executive;
  const canViewQueues = isSafetyOfficer || isHR || isExecutive;

  const [myAppointments, setMyAppointments] = useState<LegalAppointmentDto[]>([]);
  const [pendingQueue, setPendingQueue] = useState<LegalAppointmentDto[]>([]);
  const [activeAppointments, setActiveAppointments] = useState<LegalAppointmentDto[]>([]);
  const [form, setForm] = useState(initialForm);
  const [attachmentFile, setAttachmentFile] = useState<File | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isBusy, setIsBusy] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const loadAll = useCallback(async () => {
    try {
      const requests: Promise<unknown>[] = [getMyLegalAppointments().then(setMyAppointments)];
      if (canViewQueues) {
        requests.push(getPendingLegalAppointmentApprovals().then(setPendingQueue));
        requests.push(getActiveLegalAppointments().then(setActiveAppointments));
      }
      await Promise.all(requests);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  }, [canViewQueues]);

  useEffect(() => {
    loadAll();
  }, [loadAll]);

  async function handleSubmit() {
    setError(null);
    setIsSubmitting(true);
    try {
      const appointment = await submitLegalAppointment(form);
      if (attachmentFile) {
        try {
          await uploadAttachment(AttachmentEntityType.LegalAppointment, appointment.id, attachmentFile);
        } catch (err) {
          setError(
            `The appointment request was submitted, but the attachment failed to upload: ${extractErrorMessage(err)}. Attach it below.`,
          );
          await loadAll();
          return;
        }
      }
      setForm(initialForm);
      setAttachmentFile(null);
      if (fileInputRef.current) fileInputRef.current.value = "";
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  const steps: WizardStep[] = [
    {
      title: "Appointment & authority",
      validate: () => (!form.appointedBy ? "Please fill in all fields." : null),
      content: (
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Appointment type
            <select
              value={form.appointmentType}
              onChange={(e) =>
                setForm((prev) => ({ ...prev, appointmentType: Number(e.target.value) as LegalAppointmentType }))
              }
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            >
              {Object.entries(LegalAppointmentTypeLabels).map(([value, label]) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Appointed by
            <input
              required
              placeholder="e.g. J. Smith, Mine Manager"
              value={form.appointedBy}
              onChange={(e) => setForm((prev) => ({ ...prev, appointedBy: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>
        </div>
      ),
    },
    {
      title: "Validity & description",
      validate: () => (!form.validFrom || !form.validTo || !form.description ? "Please fill in all fields." : null),
      content: (
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Valid from
            <input
              type="date"
              required
              value={form.validFrom}
              onChange={(e) => setForm((prev) => ({ ...prev, validFrom: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Valid to
            <input
              type="date"
              required
              value={form.validTo}
              onChange={(e) => setForm((prev) => ({ ...prev, validTo: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700 sm:col-span-2">
            Scope of appointment
            <textarea
              required
              rows={2}
              value={form.description}
              onChange={(e) => setForm((prev) => ({ ...prev, description: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700 sm:col-span-2">
            Attachment (optional)
            <input
              ref={fileInputRef}
              type="file"
              accept="application/pdf,image/jpeg,image/png"
              onChange={(e) => setAttachmentFile(e.target.files?.[0] ?? null)}
              className="rounded-md border border-slate-300 px-3 py-1.5 text-sm file:mr-2 file:rounded file:border-0 file:bg-slate-100 file:px-2 file:py-1 file:text-xs file:font-medium file:text-slate-700 focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>
        </div>
      ),
    },
  ];

  async function handleApprove(id: string) {
    setIsBusy(true);
    try {
      await reviewLegalAppointment(id, { approve: true });
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsBusy(false);
    }
  }

  async function handleReject(id: string, reason: string) {
    setIsBusy(true);
    try {
      await reviewLegalAppointment(id, { approve: false, rejectionReason: reason || undefined });
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsBusy(false);
    }
  }

  async function handleRevoke(id: string) {
    setIsBusy(true);
    try {
      await revokeLegalAppointment(id, {});
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsBusy(false);
    }
  }

  return (
    <div className="stagger-children flex flex-col gap-6">
      {canViewQueues && (
        <>
          <LegalAppointmentApprovalQueue
            items={pendingQueue}
            isBusy={isBusy}
            canManage={isSafetyOfficer}
            onApprove={handleApprove}
            onReject={handleReject}
          />
          <LegalAppointmentRevokeQueue
            items={activeAppointments}
            isBusy={isBusy}
            canManage={isSafetyOfficer}
            onRevoke={handleRevoke}
          />
        </>
      )}

      <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <h3 className="mb-4 text-sm font-semibold text-slate-900">Request a legal appointment</h3>
        <StepForm
          steps={steps}
          onSubmit={handleSubmit}
          submitLabel="Submit request"
          submittingLabel="Submitting..."
          isSubmitting={isSubmitting}
          error={error}
        />
      </div>

      <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-200 px-6 py-4">
          <h3 className="text-sm font-semibold text-slate-900">My legal appointments</h3>
        </div>
        {myAppointments.length === 0 ? (
          <p className="px-6 py-8 text-center text-sm text-slate-500">No legal appointments yet.</p>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="text-xs uppercase tracking-wide text-slate-500">
                <th className="px-6 py-2 font-medium">Type</th>
                <th className="px-6 py-2 font-medium">Appointed by</th>
                <th className="px-6 py-2 font-medium">Valid</th>
                <th className="px-6 py-2 font-medium">Status</th>
                <th className="px-6 py-2 font-medium">Submitted</th>
                <th className="px-6 py-2 font-medium">Rejection reason</th>
                <th className="px-6 py-2 font-medium">Attachments</th>
              </tr>
            </thead>
            <tbody>
              {myAppointments.map((appointment) => (
                <tr key={appointment.id} className="border-t border-slate-100">
                  <td className="px-6 py-2 text-slate-700">{LegalAppointmentTypeLabels[appointment.appointmentType]}</td>
                  <td className="px-6 py-2 text-slate-700">{appointment.appointedBy}</td>
                  <td className="px-6 py-2 text-slate-700">
                    {formatDate(appointment.validFrom)} – {formatDate(appointment.validTo)}
                  </td>
                  <td className="px-6 py-2">
                    <LegalAppointmentStatusBadge status={appointment.status} />
                  </td>
                  <td className="px-6 py-2 text-slate-700">{formatDateTime(appointment.createdAtUtc)}</td>
                  <td className="px-6 py-2 text-slate-500">{appointment.rejectionReason ?? "—"}</td>
                  <td className="px-6 py-2">
                    <AttachmentsPanel
                      entityType={AttachmentEntityType.LegalAppointment}
                      entityId={appointment.id}
                      canUpload={isSafetyOfficer}
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
