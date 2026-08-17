import { useCallback, useEffect, useRef, useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { getSignature } from "../api/account";
import { uploadAttachment } from "../api/attachments";
import { extractErrorMessage } from "../api/client";
import {
  downloadSignedLeaveDocument,
  getMyLeaveRequests,
  getPendingHRApprovals,
  getPendingLineManagerApprovals,
  hrReview,
  lineManagerReview,
  submitLeaveRequest,
} from "../api/leave";
import { AttachmentsPanel } from "../components/AttachmentsPanel";
import { LeaveApprovalQueue } from "../components/LeaveApprovalQueue";
import { LeaveStatusBadge } from "../components/LeaveStatusBadge";
import { StepForm, type WizardStep } from "../components/StepForm";
import { formatDate, formatDateTime } from "../lib/format";
import { AttachmentEntityType } from "../types/attachment";
import { EmployeeRole } from "../types/auth";
import {
  LEAVE_TYPES_REQUIRING_CERTIFICATE,
  LeaveRequestStatus,
  LeaveType,
  LeaveTypeLabels,
  type LeaveRequestDto,
} from "../types/leave";
import { sanitizePhoneNumber } from "../utils/textInput";

interface LeaveRequestForm {
  leaveType: LeaveType;
  startDate: string;
  endDate: string;
  reason: string;
  contactNumber: string;
}

const initialForm: LeaveRequestForm = {
  leaveType: LeaveType.Annual,
  startDate: "",
  endDate: "",
  reason: "",
  contactNumber: "",
};

export function LeavePage() {
  const { session } = useAuth();
  const isLineManager = session?.role === EmployeeRole.LineManager;
  const isHR = session?.role === EmployeeRole.HR;

  const [myRequests, setMyRequests] = useState<LeaveRequestDto[]>([]);
  const [lineManagerQueue, setLineManagerQueue] = useState<LeaveRequestDto[]>([]);
  const [hrQueue, setHrQueue] = useState<LeaveRequestDto[]>([]);
  const [form, setForm] = useState(initialForm);
  const [attachmentFile, setAttachmentFile] = useState<File | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isReviewing, setIsReviewing] = useState(false);
  const [hasSavedSignature, setHasSavedSignature] = useState(true);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const loadAll = useCallback(async () => {
    try {
      const requests: Promise<unknown>[] = [getMyLeaveRequests().then(setMyRequests)];
      if (isLineManager) requests.push(getPendingLineManagerApprovals().then(setLineManagerQueue));
      if (isHR) requests.push(getPendingHRApprovals().then(setHrQueue));
      await Promise.all(requests);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  }, [isLineManager, isHR]);

  useEffect(() => {
    loadAll();
  }, [loadAll]);

  useEffect(() => {
    getSignature()
      .then((signature) => setHasSavedSignature(signature.hasSignature))
      .catch(() => {
        // Assume a signature exists so the pad doesn't flash in for the common case.
      });
  }, []);

  async function handleSubmit() {
    setError(null);
    setIsSubmitting(true);
    try {
      const request = await submitLeaveRequest(form);
      if (attachmentFile) {
        try {
          await uploadAttachment(AttachmentEntityType.LeaveRequest, request.id, attachmentFile);
        } catch (err) {
          setError(
            `The leave request was submitted, but the attachment failed to upload: ${extractErrorMessage(err)}. Attach it below.`,
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
      title: "Leave details",
      validate: () => (!form.startDate || !form.endDate ? "Please fill in all fields." : null),
      content: (
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm text-slate-700 sm:col-span-2">
            Leave type
            <select
              value={form.leaveType}
              onChange={(e) => setForm((prev) => ({ ...prev, leaveType: Number(e.target.value) as LeaveType }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            >
              {Object.entries(LeaveTypeLabels).map(([value, label]) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </select>
            {LEAVE_TYPES_REQUIRING_CERTIFICATE.includes(form.leaveType) && (
              <span className="text-xs text-amber-600">Attach a medical certificate on the next step.</span>
            )}
          </label>

          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Start date
            <input
              type="date"
              required
              value={form.startDate}
              onChange={(e) => setForm((prev) => ({ ...prev, startDate: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-slate-700">
            End date
            <input
              type="date"
              required
              value={form.endDate}
              onChange={(e) => setForm((prev) => ({ ...prev, endDate: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>
        </div>
      ),
    },
    {
      title: "Reason & contact",
      validate: () => (!form.reason || !form.contactNumber ? "Please fill in all fields." : null),
      content: (
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm text-slate-700 sm:col-span-2">
            Reason
            <input
              required
              value={form.reason}
              onChange={(e) => setForm((prev) => ({ ...prev, reason: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-slate-700 sm:col-span-2">
            Contact number during leave
            <input
              type="tel"
              required
              value={form.contactNumber}
              onChange={(e) => setForm((prev) => ({ ...prev, contactNumber: sanitizePhoneNumber(e.target.value) }))}
              placeholder="e.g. 082 000 0000"
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-slate-700 sm:col-span-2">
            Attachment (optional{LEAVE_TYPES_REQUIRING_CERTIFICATE.includes(form.leaveType) ? " — medical certificate" : ""})
            <input
              ref={fileInputRef}
              type="file"
              accept="application/pdf,image/jpeg,image/png"
              onChange={(e) => setAttachmentFile(e.target.files?.[0] ?? null)}
              className="rounded-md border border-slate-300 px-3 py-1.5 text-sm file:mr-2 file:rounded file:border-0 file:bg-slate-100 file:px-2 file:py-1 file:text-xs file:font-medium file:text-slate-700 focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>

          <p className="text-xs text-slate-500 sm:col-span-2">
            You must seek approval for leave, other than sick leave, at least 2 days prior to your day of leave. On
            the first day of sick leave, inform your Manager/Supervisor before 6AM that you are sick.
          </p>
        </div>
      ),
    },
  ];

  async function handleLineManagerApprove(id: string, signaturePngBase64?: string) {
    setIsReviewing(true);
    try {
      await lineManagerReview(id, { approve: true, signaturePngBase64 });
      setHasSavedSignature(true);
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsReviewing(false);
    }
  }

  async function handleLineManagerReject(id: string, reason: string) {
    setIsReviewing(true);
    try {
      await lineManagerReview(id, { approve: false, rejectionReason: reason || undefined });
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsReviewing(false);
    }
  }

  async function handleHRApprove(id: string, signaturePngBase64?: string) {
    setIsReviewing(true);
    try {
      await hrReview(id, { approve: true, signaturePngBase64 });
      setHasSavedSignature(true);
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsReviewing(false);
    }
  }

  async function handleHRReject(id: string, reason: string) {
    setIsReviewing(true);
    try {
      await hrReview(id, { approve: false, rejectionReason: reason || undefined });
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsReviewing(false);
    }
  }

  async function handleDownloadSignedDocument(request: LeaveRequestDto) {
    try {
      await downloadSignedLeaveDocument(request.id, `leave-request-${request.employeeName}-${request.startDate}.pdf`);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  }

  return (
    <div className="stagger-children flex flex-col gap-6">
      {isLineManager && (
        <LeaveApprovalQueue
          title="Pending my approval (Line Manager)"
          items={lineManagerQueue}
          isBusy={isReviewing}
          hasSavedSignature={hasSavedSignature}
          onApprove={handleLineManagerApprove}
          onReject={handleLineManagerReject}
        />
      )}

      {isHR && (
        <LeaveApprovalQueue
          title="Pending HR approval"
          items={hrQueue}
          isBusy={isReviewing}
          hasSavedSignature={hasSavedSignature}
          onApprove={handleHRApprove}
          onReject={handleHRReject}
        />
      )}

      <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <h3 className="mb-4 text-sm font-semibold text-slate-900">Apply for leave</h3>
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
          <h3 className="text-sm font-semibold text-slate-900">My requests</h3>
        </div>
        {myRequests.length === 0 ? (
          <p className="px-6 py-8 text-center text-sm text-slate-500">No leave requests yet.</p>
        ) : (
          <ul className="divide-y divide-slate-100">
            {myRequests.map((request) => (
              <li key={request.id} className="px-6 py-4">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <p className="flex items-center gap-2 text-sm font-medium text-slate-900">
                      {LeaveTypeLabels[request.leaveType]}
                      <LeaveStatusBadge status={request.status} />
                    </p>
                    <p className="text-sm text-slate-600">
                      {formatDate(request.startDate)} – {formatDate(request.endDate)} ({request.daysRequested}{" "}
                      {request.daysRequested === 1 ? "day" : "days"})
                    </p>
                    <p className="mt-1 text-xs text-slate-500">
                      Contact during leave: {request.contactNumber} · Submitted {formatDateTime(request.createdAtUtc)}
                    </p>
                    {request.rejectionReason && (
                      <p className="mt-1 text-xs text-red-600">Rejection reason: {request.rejectionReason}</p>
                    )}
                  </div>
                  {request.status === LeaveRequestStatus.Approved && (
                    <button
                      type="button"
                      onClick={() => handleDownloadSignedDocument(request)}
                      className="text-xs font-medium text-yellow-700 hover:underline"
                    >
                      Download signed form
                    </button>
                  )}
                </div>

                <AttachmentsPanel entityType={AttachmentEntityType.LeaveRequest} entityId={request.id} canUpload />
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
