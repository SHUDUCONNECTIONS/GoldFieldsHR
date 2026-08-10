import { useCallback, useEffect, useState, type FormEvent } from "react";
import { useAuth } from "../auth/AuthContext";
import { extractErrorMessage } from "../api/client";
import {
  getMyShiftChangeRequests,
  getPendingHRApprovals,
  getPendingLineManagerApprovals,
  hrReview,
  lineManagerReview,
  submitShiftChangeRequest,
} from "../api/workShift";
import { ShiftApprovalQueue } from "../components/ShiftApprovalQueue";
import { StatusBadge } from "../components/StatusBadge";
import { formatDate, formatDateTime } from "../lib/format";
import { EmployeeRole } from "../types/auth";
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

  const loadAll = useCallback(async () => {
    try {
      const requests: Promise<unknown>[] = [getMyShiftChangeRequests().then(setMyRequests)];
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

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
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

  return (
    <div className="flex flex-col gap-6">
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
        <form onSubmit={handleSubmit} className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Requested date
            <input
              type="date"
              required
              value={form.requestedShiftDate}
              onChange={(e) => setForm((prev) => ({ ...prev, requestedShiftDate: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Shift
            <select
              value={form.requestedShiftType}
              onChange={(e) =>
                setForm((prev) => ({ ...prev, requestedShiftType: Number(e.target.value) as ShiftType }))
              }
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
            >
              {Object.entries(ShiftTypeLabels).map(([value, label]) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </select>
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
            Comments (optional)
            <textarea
              value={form.comments}
              onChange={(e) => setForm((prev) => ({ ...prev, comments: e.target.value }))}
              rows={2}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
            />
          </label>

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
