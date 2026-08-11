import { useCallback, useEffect, useState, type FormEvent } from "react";
import { useAuth } from "../auth/AuthContext";
import { extractErrorMessage } from "../api/client";
import {
  getMyLeaveRequests,
  getPendingLeaveApprovals,
  reviewLeaveRequest,
  submitLeaveRequest,
} from "../api/leave";
import { AttachmentsPanel } from "../components/AttachmentsPanel";
import { LeaveApprovalQueue } from "../components/LeaveApprovalQueue";
import { LeaveStatusBadge } from "../components/LeaveStatusBadge";
import { formatDate, formatDateTime } from "../lib/format";
import { AttachmentEntityType } from "../types/attachment";
import { EmployeeRole } from "../types/auth";
import {
  LEAVE_TYPES_REQUIRING_CERTIFICATE,
  LeaveType,
  LeaveTypeLabels,
  type LeaveRequestDto,
} from "../types/leave";

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

  const [myRequests, setMyRequests] = useState<LeaveRequestDto[]>([]);
  const [pendingQueue, setPendingQueue] = useState<LeaveRequestDto[]>([]);
  const [form, setForm] = useState(initialForm);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isReviewing, setIsReviewing] = useState(false);

  const loadAll = useCallback(async () => {
    try {
      const requests: Promise<unknown>[] = [getMyLeaveRequests().then(setMyRequests)];
      if (isLineManager) requests.push(getPendingLeaveApprovals().then(setPendingQueue));
      await Promise.all(requests);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  }, [isLineManager]);

  useEffect(() => {
    loadAll();
  }, [loadAll]);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      await submitLeaveRequest(form);
      setForm(initialForm);
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleApprove(id: string) {
    setIsReviewing(true);
    try {
      await reviewLeaveRequest(id, { approve: true });
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsReviewing(false);
    }
  }

  async function handleReject(id: string, reason: string) {
    setIsReviewing(true);
    try {
      await reviewLeaveRequest(id, { approve: false, rejectionReason: reason || undefined });
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsReviewing(false);
    }
  }

  return (
    <div className="stagger-children flex flex-col gap-6">
      {isLineManager && (
        <LeaveApprovalQueue
          items={pendingQueue}
          isBusy={isReviewing}
          onApprove={handleApprove}
          onReject={handleReject}
        />
      )}

      <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <h3 className="mb-4 text-sm font-semibold text-slate-900">Apply for leave</h3>
        <form onSubmit={handleSubmit} className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm text-slate-700 sm:col-span-2">
            Leave type
            <select
              value={form.leaveType}
              onChange={(e) => setForm((prev) => ({ ...prev, leaveType: Number(e.target.value) as LeaveType }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
            >
              {Object.entries(LeaveTypeLabels).map(([value, label]) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </select>
            {LEAVE_TYPES_REQUIRING_CERTIFICATE.includes(form.leaveType) && (
              <span className="text-xs text-amber-600">
                Attach a medical certificate below after submitting.
              </span>
            )}
          </label>

          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Start date
            <input
              type="date"
              required
              value={form.startDate}
              onChange={(e) => setForm((prev) => ({ ...prev, startDate: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-slate-700">
            End date
            <input
              type="date"
              required
              value={form.endDate}
              onChange={(e) => setForm((prev) => ({ ...prev, endDate: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-slate-700 sm:col-span-2">
            Reason
            <input
              required
              value={form.reason}
              onChange={(e) => setForm((prev) => ({ ...prev, reason: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-slate-700 sm:col-span-2">
            Contact number during leave
            <input
              type="tel"
              required
              value={form.contactNumber}
              onChange={(e) => setForm((prev) => ({ ...prev, contactNumber: e.target.value }))}
              placeholder="e.g. 082 000 0000"
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
            />
          </label>

          <p className="text-xs text-slate-500 sm:col-span-2">
            You must seek approval for leave, other than sick leave, at least 2 days prior to your day of leave. On
            the first day of sick leave, inform your Manager/Supervisor before 6AM that you are sick.
          </p>

          {error && <p className="text-sm text-red-600 sm:col-span-2">{error}</p>}

          <button
            type="submit"
            disabled={isSubmitting}
            className="mt-1 w-fit rounded-md bg-slate-950 px-4 py-2 text-sm font-medium text-white hover:bg-slate-800 disabled:opacity-50 sm:col-span-2"
          >
            {isSubmitting ? "Submitting..." : "Submit request"}
          </button>
        </form>
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
