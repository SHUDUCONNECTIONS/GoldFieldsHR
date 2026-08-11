import { useCallback, useEffect, useState, type FormEvent } from "react";
import { useAuth } from "../auth/AuthContext";
import { extractErrorMessage } from "../api/client";
import {
  getMyPpeRequests,
  getPendingPpeApprovals,
  getPpeAwaitingIssue,
  issuePpeRequest,
  reviewPpeRequest,
  submitPpeRequest,
} from "../api/ppe";
import { PpeApprovalQueue } from "../components/PpeApprovalQueue";
import { PpeIssueQueue } from "../components/PpeIssueQueue";
import { PpeStatusBadge } from "../components/PpeStatusBadge";
import { formatDateTime } from "../lib/format";
import { EmployeeRole } from "../types/auth";
import { PpeItemType, PpeItemTypeLabels, type PpeRequestDto } from "../types/ppe";

interface PpeRequestForm {
  itemType: PpeItemType;
  size: string;
  quantity: number;
  reason: string;
}

const initialForm: PpeRequestForm = {
  itemType: PpeItemType.Helmet,
  size: "",
  quantity: 1,
  reason: "",
};

export function PpePage() {
  const { session } = useAuth();
  const isSafetyOfficer = session?.role === EmployeeRole.SafetyOfficer;

  const [myRequests, setMyRequests] = useState<PpeRequestDto[]>([]);
  const [pendingQueue, setPendingQueue] = useState<PpeRequestDto[]>([]);
  const [awaitingIssue, setAwaitingIssue] = useState<PpeRequestDto[]>([]);
  const [form, setForm] = useState(initialForm);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isBusy, setIsBusy] = useState(false);

  const loadAll = useCallback(async () => {
    try {
      const requests: Promise<unknown>[] = [getMyPpeRequests().then(setMyRequests)];
      if (isSafetyOfficer) {
        requests.push(getPendingPpeApprovals().then(setPendingQueue));
        requests.push(getPpeAwaitingIssue().then(setAwaitingIssue));
      }
      await Promise.all(requests);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  }, [isSafetyOfficer]);

  useEffect(() => {
    loadAll();
  }, [loadAll]);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      await submitPpeRequest({ ...form, size: form.size || undefined });
      setForm(initialForm);
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleApprove(id: string) {
    setIsBusy(true);
    try {
      await reviewPpeRequest(id, { approve: true });
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
      await reviewPpeRequest(id, { approve: false, rejectionReason: reason || undefined });
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsBusy(false);
    }
  }

  async function handleIssue(id: string) {
    setIsBusy(true);
    try {
      await issuePpeRequest(id);
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsBusy(false);
    }
  }

  return (
    <div className="stagger-children flex flex-col gap-6">
      {isSafetyOfficer && (
        <>
          <PpeApprovalQueue
            items={pendingQueue}
            isBusy={isBusy}
            onApprove={handleApprove}
            onReject={handleReject}
          />
          <PpeIssueQueue items={awaitingIssue} isBusy={isBusy} onIssue={handleIssue} />
        </>
      )}

      <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <h3 className="mb-4 text-sm font-semibold text-slate-900">Request PPE</h3>
        <form onSubmit={handleSubmit} className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Item
            <select
              value={form.itemType}
              onChange={(e) => setForm((prev) => ({ ...prev, itemType: Number(e.target.value) as PpeItemType }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
            >
              {Object.entries(PpeItemTypeLabels).map(([value, label]) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Size (optional)
            <input
              value={form.size}
              onChange={(e) => setForm((prev) => ({ ...prev, size: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Quantity
            <input
              type="number"
              min={1}
              max={20}
              required
              value={form.quantity}
              onChange={(e) => setForm((prev) => ({ ...prev, quantity: Number(e.target.value) }))}
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
          <p className="px-6 py-8 text-center text-sm text-slate-500">No PPE requests yet.</p>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="text-xs uppercase tracking-wide text-slate-500">
                <th className="px-6 py-2 font-medium">Item</th>
                <th className="px-6 py-2 font-medium">Size</th>
                <th className="px-6 py-2 font-medium">Qty</th>
                <th className="px-6 py-2 font-medium">Status</th>
                <th className="px-6 py-2 font-medium">Submitted</th>
                <th className="px-6 py-2 font-medium">Rejection reason</th>
              </tr>
            </thead>
            <tbody>
              {myRequests.map((request) => (
                <tr key={request.id} className="border-t border-slate-100">
                  <td className="px-6 py-2 text-slate-700">{PpeItemTypeLabels[request.itemType]}</td>
                  <td className="px-6 py-2 text-slate-700">{request.size ?? "—"}</td>
                  <td className="px-6 py-2 text-slate-700">{request.quantity}</td>
                  <td className="px-6 py-2">
                    <PpeStatusBadge status={request.status} />
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
