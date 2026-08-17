import { useCallback, useEffect, useRef, useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { uploadAttachment } from "../api/attachments";
import { extractErrorMessage } from "../api/client";
import {
  getMyShiftChangeRequests,
  getPendingHRApprovals,
  getPendingLineManagerApprovals,
  hrReview,
  lineManagerReview,
  submitShiftChangeRequest,
} from "../api/workShift";
import { createScheduleDocument, deleteScheduleDocument, getScheduleDocuments } from "../api/scheduleDocuments";
import { AttachmentsPanel } from "../components/AttachmentsPanel";
import { ShiftApprovalQueue } from "../components/ShiftApprovalQueue";
import { StatusBadge } from "../components/StatusBadge";
import { StepForm, type WizardStep } from "../components/StepForm";
import { formatDate, formatDateTime } from "../lib/format";
import { AttachmentEntityType } from "../types/attachment";
import { EmployeeRole } from "../types/auth";
import type { PostedScheduleDocumentDto } from "../types/scheduleDocument";
import { ShiftType, ShiftTypeLabels, type ShiftChangeRequestDto } from "../types/workShift";

interface ShiftRequestForm {
  requestedShiftDate: string;
  requestedShiftType: ShiftType;
  reason: string;
  comments: string;
}

const initialForm: ShiftRequestForm = {
  requestedShiftDate: "",
  requestedShiftType: ShiftType.Day,
  reason: "",
  comments: "",
};

export function WorkShiftPage() {
  const { session } = useAuth();
  const isLineManager = session?.role === EmployeeRole.LineManager;
  const isHR = session?.role === EmployeeRole.HR;

  const [myRequests, setMyRequests] = useState<ShiftChangeRequestDto[]>([]);
  const [lineManagerQueue, setLineManagerQueue] = useState<ShiftChangeRequestDto[]>([]);
  const [hrQueue, setHrQueue] = useState<ShiftChangeRequestDto[]>([]);
  const [form, setForm] = useState(initialForm);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isReviewing, setIsReviewing] = useState(false);

  const [scheduleDocuments, setScheduleDocuments] = useState<PostedScheduleDocumentDto[]>([]);
  const [scheduleTitle, setScheduleTitle] = useState("");
  const [scheduleFile, setScheduleFile] = useState<File | null>(null);
  const [isPostingSchedule, setIsPostingSchedule] = useState(false);
  const [scheduleError, setScheduleError] = useState<string | null>(null);
  const [deletingDocumentId, setDeletingDocumentId] = useState<string | null>(null);
  const scheduleFileInputRef = useRef<HTMLInputElement>(null);

  const loadAll = useCallback(async () => {
    try {
      const requests: Promise<unknown>[] = [
        getMyShiftChangeRequests().then(setMyRequests),
        getScheduleDocuments().then(setScheduleDocuments),
      ];
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

  async function handleSubmit() {
    setError(null);
    setIsSubmitting(true);
    try {
      await submitShiftChangeRequest({
        requestedShiftDate: form.requestedShiftDate,
        requestedShiftType: form.requestedShiftType,
        reason: form.reason,
        comments: form.comments || undefined,
      });
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
      title: "Shift",
      validate: () => (!form.requestedShiftDate ? "Please fill in all fields." : null),
      content: (
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Requested date
            <input
              type="date"
              required
              value={form.requestedShiftDate}
              onChange={(e) => setForm((prev) => ({ ...prev, requestedShiftDate: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Shift
            <select
              value={form.requestedShiftType}
              onChange={(e) =>
                setForm((prev) => ({ ...prev, requestedShiftType: Number(e.target.value) as ShiftType }))
              }
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            >
              {Object.entries(ShiftTypeLabels).map(([value, label]) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </select>
          </label>
        </div>
      ),
    },
    {
      title: "Reason",
      validate: () => (!form.reason ? "Please fill in all fields." : null),
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
            Comments (optional)
            <textarea
              value={form.comments}
              onChange={(e) => setForm((prev) => ({ ...prev, comments: e.target.value }))}
              rows={2}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>
        </div>
      ),
    },
  ];

  async function handleLineManagerApprove(id: string) {
    setIsReviewing(true);
    try {
      await lineManagerReview(id, { approve: true });
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

  async function handleHRApprove(id: string) {
    setIsReviewing(true);
    try {
      await hrReview(id, { approve: true });
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

  async function handlePostScheduleDocument() {
    if (!scheduleTitle.trim()) {
      setScheduleError("Give the schedule a title, e.g. \"Week 34 Roster\".");
      return;
    }
    setScheduleError(null);
    setIsPostingSchedule(true);
    try {
      const document = await createScheduleDocument({ title: scheduleTitle.trim() });
      if (scheduleFile) {
        try {
          await uploadAttachment(AttachmentEntityType.WorkShiftSchedule, document.id, scheduleFile);
        } catch (err) {
          setScheduleError(
            `The schedule was posted, but the attachment failed to upload: ${extractErrorMessage(err)}. Attach it below.`,
          );
          await loadAll();
          return;
        }
      }
      setScheduleTitle("");
      setScheduleFile(null);
      if (scheduleFileInputRef.current) scheduleFileInputRef.current.value = "";
      await loadAll();
    } catch (err) {
      setScheduleError(extractErrorMessage(err));
    } finally {
      setIsPostingSchedule(false);
    }
  }

  async function handleDeleteScheduleDocument(id: string) {
    setDeletingDocumentId(id);
    try {
      await deleteScheduleDocument(id);
      await loadAll();
    } catch (err) {
      setScheduleError(extractErrorMessage(err));
    } finally {
      setDeletingDocumentId(null);
    }
  }

  return (
    <div className="stagger-children flex flex-col gap-6">
      {isHR && (
        <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
          <h3 className="mb-1 text-sm font-semibold text-slate-900">Post a schedule</h3>
          <p className="mb-4 text-xs text-slate-500">
            Upload the roster as a document (PDF or image) — everyone can view and download it below.
          </p>
          <div className="flex flex-wrap items-end gap-3">
            <label className="flex flex-1 min-w-[220px] flex-col gap-1 text-sm text-slate-700">
              Title
              <input
                value={scheduleTitle}
                onChange={(e) => setScheduleTitle(e.target.value)}
                placeholder="e.g. Week 34 Roster"
                className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
              />
            </label>
            <label className="flex flex-1 min-w-[220px] flex-col gap-1 text-sm text-slate-700">
              Attachment (optional)
              <input
                ref={scheduleFileInputRef}
                type="file"
                accept="application/pdf,image/jpeg,image/png"
                onChange={(e) => setScheduleFile(e.target.files?.[0] ?? null)}
                className="rounded-md border border-slate-300 px-3 py-1.5 text-sm file:mr-2 file:rounded file:border-0 file:bg-slate-100 file:px-2 file:py-1 file:text-xs file:font-medium file:text-slate-700 focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
              />
            </label>
            <button
              type="button"
              disabled={isPostingSchedule}
              onClick={handlePostScheduleDocument}
              className="rounded-md bg-yellow-600 px-4 py-2 text-sm font-medium text-white hover:bg-yellow-500 disabled:opacity-50"
            >
              {isPostingSchedule ? "Posting..." : "Post schedule"}
            </button>
          </div>
          {scheduleError && <p className="mt-2 text-sm text-red-600">{scheduleError}</p>}
        </div>
      )}

      <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-200 px-6 py-4">
          <h3 className="text-sm font-semibold text-slate-900">Posted schedules</h3>
        </div>
        {scheduleDocuments.length === 0 ? (
          <p className="px-6 py-8 text-center text-sm text-slate-500">No schedules posted yet.</p>
        ) : (
          <ul className="divide-y divide-slate-100">
            {scheduleDocuments.map((doc) => (
              <li key={doc.id} className="px-6 py-4">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <p className="text-sm font-medium text-slate-900">{doc.title}</p>
                    <p className="text-xs text-slate-500">
                      Posted by {doc.postedByName} — {formatDateTime(doc.postedAtUtc)}
                    </p>
                  </div>
                  {isHR && (
                    <button
                      type="button"
                      disabled={deletingDocumentId !== null}
                      onClick={() => handleDeleteScheduleDocument(doc.id)}
                      className="text-xs font-medium text-red-600 hover:underline disabled:opacity-50"
                    >
                      Remove
                    </button>
                  )}
                </div>
                <AttachmentsPanel entityType={AttachmentEntityType.WorkShiftSchedule} entityId={doc.id} canUpload={isHR} />
              </li>
            ))}
          </ul>
        )}
      </div>

      {isLineManager && (
        <ShiftApprovalQueue
          title="Pending my approval (Line Manager)"
          items={lineManagerQueue}
          isBusy={isReviewing}
          onApprove={handleLineManagerApprove}
          onReject={handleLineManagerReject}
        />
      )}

      {isHR && (
        <ShiftApprovalQueue
          title="Pending HR approval"
          items={hrQueue}
          isBusy={isReviewing}
          onApprove={handleHRApprove}
          onReject={handleHRReject}
        />
      )}

      <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <h3 className="mb-4 text-sm font-semibold text-slate-900">Request a shift change</h3>
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
          <p className="px-6 py-8 text-center text-sm text-slate-500">No shift change requests yet.</p>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="text-xs uppercase tracking-wide text-slate-500">
                <th className="px-6 py-2 font-medium">Requested date</th>
                <th className="px-6 py-2 font-medium">Shift</th>
                <th className="px-6 py-2 font-medium">Status</th>
                <th className="px-6 py-2 font-medium">Submitted</th>
                <th className="px-6 py-2 font-medium">Rejection reason</th>
              </tr>
            </thead>
            <tbody>
              {myRequests.map((request) => (
                <tr key={request.id} className="border-t border-slate-100">
                  <td className="px-6 py-2 text-slate-700">{formatDate(request.requestedShiftDate)}</td>
                  <td className="px-6 py-2 text-slate-700">{ShiftTypeLabels[request.requestedShiftType]}</td>
                  <td className="px-6 py-2">
                    <StatusBadge status={request.status} />
                  </td>
                  <td className="px-6 py-2 text-slate-700">{formatDateTime(request.createdAtUtc)}</td>
                  <td className="px-6 py-2 text-slate-500">{request.rejectionReason ?? "—"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
