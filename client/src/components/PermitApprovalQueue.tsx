import { useState } from "react";
import { formatDate } from "../lib/format";
import { PermitTypeLabels, type WorkPermitDto } from "../types/permit";

interface PermitApprovalQueueProps {
  items: WorkPermitDto[];
  isBusy: boolean;
  onApprove: (id: string) => void;
  onReject: (id: string, reason: string) => void;
}

export function PermitApprovalQueue({ items, isBusy, onApprove, onReject }: PermitApprovalQueueProps) {
  const [rejectingId, setRejectingId] = useState<string | null>(null);
  const [reason, setReason] = useState("");

  function startReject(id: string) {
    setRejectingId(id);
    setReason("");
  }

  function confirmReject(id: string) {
    onReject(id, reason);
    setRejectingId(null);
    setReason("");
  }

  return (
    <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
      <div className="border-b border-slate-200 px-6 py-4">
        <h3 className="text-sm font-semibold text-slate-900">Pending my approval (Safety Officer)</h3>
      </div>
      {items.length === 0 ? (
        <p className="px-6 py-8 text-center text-sm text-slate-500">Nothing pending approval.</p>
      ) : (
        <ul className="divide-y divide-slate-100">
          {items.map((item) => (
            <li key={item.id} className="px-6 py-4">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <p className="text-sm font-medium text-slate-900">{item.employeeName}</p>
                  <p className="text-sm text-slate-600">
                    {PermitTypeLabels[item.permitType]} — {item.location} — {formatDate(item.validFrom)} to{" "}
                    {formatDate(item.validTo)}
                  </p>
                  <p className="mt-1 text-xs text-slate-500">{item.description}</p>
                </div>
                <div className="flex gap-2">
                  <button
                    type="button"
                    disabled={isBusy}
                    onClick={() => onApprove(item.id)}
                    className="rounded-md bg-emerald-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-emerald-500 disabled:opacity-50"
                  >
                    Approve
                  </button>
                  <button
                    type="button"
                    disabled={isBusy}
                    onClick={() => startReject(item.id)}
                    className="rounded-md bg-red-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-red-500 disabled:opacity-50"
                  >
                    Reject
                  </button>
                </div>
              </div>

              {rejectingId === item.id && (
                <div className="mt-3 flex flex-wrap items-center gap-2">
                  <input
                    type="text"
                    placeholder="Reason for rejection (optional)"
                    value={reason}
                    onChange={(e) => setReason(e.target.value)}
                    className="min-w-[240px] flex-1 rounded-md border border-slate-300 px-2 py-1 text-xs focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
                  />
                  <button
                    type="button"
                    disabled={isBusy}
                    onClick={() => confirmReject(item.id)}
                    className="rounded-md bg-red-600 px-3 py-1 text-xs font-medium text-white hover:bg-red-500 disabled:opacity-50"
                  >
                    Confirm reject
                  </button>
                  <button
                    type="button"
                    onClick={() => setRejectingId(null)}
                    className="text-xs text-slate-500 hover:underline"
                  >
                    Cancel
                  </button>
                </div>
              )}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
