import { useRef, useState } from "react";
import { AttachmentsPanel } from "./AttachmentsPanel";
import { SignaturePad, type SignaturePadHandle } from "./SignaturePad";
import { formatDate } from "../lib/format";
import { AttachmentEntityType } from "../types/attachment";
import { LeaveTypeLabels, type LeaveRequestDto } from "../types/leave";

interface LeaveApprovalQueueProps {
  title: string;
  items: LeaveRequestDto[];
  isBusy: boolean;
  hasSavedSignature: boolean;
  onApprove: (id: string, signaturePngBase64?: string) => void;
  onReject: (id: string, reason: string) => void;
}

export function LeaveApprovalQueue({ title, items, isBusy, hasSavedSignature, onApprove, onReject }: LeaveApprovalQueueProps) {
  const [rejectingId, setRejectingId] = useState<string | null>(null);
  const [reason, setReason] = useState("");
  const [approvingId, setApprovingId] = useState<string | null>(null);
  const [signError, setSignError] = useState<string | null>(null);
  const signaturePadRef = useRef<SignaturePadHandle>(null);

  function startReject(id: string) {
    setApprovingId(null);
    setRejectingId(id);
    setReason("");
  }

  function confirmReject(id: string) {
    onReject(id, reason);
    setRejectingId(null);
    setReason("");
  }

  function handleApproveClick(id: string) {
    if (hasSavedSignature) {
      onApprove(id);
      return;
    }
    setRejectingId(null);
    setSignError(null);
    setApprovingId(id);
  }

  function confirmApprove(id: string) {
    const drawn = signaturePadRef.current?.getSignature();
    if (!drawn) {
      setSignError("Please sign to approve this request.");
      return;
    }
    onApprove(id, drawn);
    setApprovingId(null);
  }

  return (
    <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
      <div className="border-b border-slate-200 px-6 py-4">
        <h3 className="text-sm font-semibold text-slate-900">{title}</h3>
      </div>
      {items.length === 0 ? (
        <p className="px-6 py-8 text-center text-sm text-slate-500">Nothing pending approval.</p>
      ) : (
        <ul className="divide-y divide-slate-100">
          {items.map((item) => (
            <li key={item.id} className="px-6 py-4">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <p className="flex items-center gap-2 text-sm font-medium text-slate-900">
                    {item.employeeName}
                    {item.isDirectReport && (
                      <span className="rounded-full bg-amber-100 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-amber-700">
                        My team
                      </span>
                    )}
                  </p>
                  <p className="text-sm text-slate-600">
                    {LeaveTypeLabels[item.leaveType]} — {formatDate(item.startDate)} to{" "}
                    {formatDate(item.endDate)} ({item.daysRequested}{" "}
                    {item.daysRequested === 1 ? "day" : "days"})
                  </p>
                  <p className="mt-1 text-xs text-slate-500">Reason: {item.reason}</p>
                  <p className="text-xs text-slate-500">Contact during leave: {item.contactNumber}</p>
                </div>
                <div className="flex gap-2">
                  <button
                    type="button"
                    disabled={isBusy}
                    onClick={() => handleApproveClick(item.id)}
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

              <AttachmentsPanel
                entityType={AttachmentEntityType.LeaveRequest}
                entityId={item.id}
                canUpload={false}
                compact
              />

              {approvingId === item.id && (
                <div className="mt-3 max-w-sm rounded-md border border-slate-200 bg-slate-50 p-3">
                  <p className="mb-1 text-xs font-medium text-slate-700">Sign to approve</p>
                  <SignaturePad ref={signaturePadRef} height={110} />
                  {signError && <p className="mt-1 text-xs text-red-600">{signError}</p>}
                  <div className="mt-2 flex gap-2">
                    <button
                      type="button"
                      disabled={isBusy}
                      onClick={() => confirmApprove(item.id)}
                      className="rounded-md bg-emerald-600 px-3 py-1 text-xs font-medium text-white hover:bg-emerald-500 disabled:opacity-50"
                    >
                      Confirm & approve
                    </button>
                    <button
                      type="button"
                      onClick={() => setApprovingId(null)}
                      className="text-xs text-slate-500 hover:underline"
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              )}

              {rejectingId === item.id && (
                <div className="mt-3 flex flex-wrap items-center gap-2">
                  <input
                    type="text"
                    placeholder="Reason for rejection (optional)"
                    value={reason}
                    onChange={(e) => setReason(e.target.value)}
                    className="min-w-[240px] flex-1 rounded-md border border-slate-300 px-2 py-1 text-xs focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
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
