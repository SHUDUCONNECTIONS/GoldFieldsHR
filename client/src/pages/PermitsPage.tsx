import { useCallback, useEffect, useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { extractErrorMessage } from "../api/client";
import {
  closePermit,
  getMyPermits,
  getOpenPermits,
  getPendingPermitApprovals,
  reviewPermit,
  submitPermitRequest,
} from "../api/permits";
import { PermitApprovalQueue } from "../components/PermitApprovalQueue";
import { PermitCloseQueue } from "../components/PermitCloseQueue";
import { PermitStatusBadge } from "../components/PermitStatusBadge";
import { StepForm, type WizardStep } from "../components/StepForm";
import { formatDate, formatDateTime } from "../lib/format";
import { EmployeeRole } from "../types/auth";
import { PermitType, PermitTypeLabels, type WorkPermitDto } from "../types/permit";

interface PermitRequestForm {
  permitType: PermitType;
  location: string;
  description: string;
  validFrom: string;
  validTo: string;
}

const initialForm: PermitRequestForm = {
  permitType: PermitType.HotWork,
  location: "",
  description: "",
  validFrom: "",
  validTo: "",
};

export function PermitsPage() {
  const { session } = useAuth();
  const isSafetyOfficer = session?.role === EmployeeRole.SafetyOfficer;

  const [myPermits, setMyPermits] = useState<WorkPermitDto[]>([]);
  const [pendingQueue, setPendingQueue] = useState<WorkPermitDto[]>([]);
  const [openPermits, setOpenPermits] = useState<WorkPermitDto[]>([]);
  const [form, setForm] = useState(initialForm);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isBusy, setIsBusy] = useState(false);

  const loadAll = useCallback(async () => {
    try {
      const requests: Promise<unknown>[] = [getMyPermits().then(setMyPermits)];
      if (isSafetyOfficer) {
        requests.push(getPendingPermitApprovals().then(setPendingQueue));
        requests.push(getOpenPermits().then(setOpenPermits));
      }
      await Promise.all(requests);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  }, [isSafetyOfficer]);

  useEffect(() => {
    loadAll();
  }, [loadAll]);

  async function handleSubmit() {
    setError(null);
    setIsSubmitting(true);
    try {
      await submitPermitRequest(form);
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
      title: "Permit & location",
      validate: () => (!form.location ? "Please fill in all fields." : null),
      content: (
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Permit type
            <select
              value={form.permitType}
              onChange={(e) => setForm((prev) => ({ ...prev, permitType: Number(e.target.value) as PermitType }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
            >
              {Object.entries(PermitTypeLabels).map(([value, label]) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Location
            <input
              required
              value={form.location}
              onChange={(e) => setForm((prev) => ({ ...prev, location: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
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
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Valid to
            <input
              type="date"
              required
              value={form.validTo}
              onChange={(e) => setForm((prev) => ({ ...prev, validTo: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700 sm:col-span-2">
            Description of work
            <textarea
              required
              rows={2}
              value={form.description}
              onChange={(e) => setForm((prev) => ({ ...prev, description: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
            />
          </label>
        </div>
      ),
    },
  ];

  async function handleApprove(id: string) {
    setIsBusy(true);
    try {
      await reviewPermit(id, { approve: true });
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
      await reviewPermit(id, { approve: false, rejectionReason: reason || undefined });
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsBusy(false);
    }
  }

  async function handleClose(id: string) {
    setIsBusy(true);
    try {
      await closePermit(id, {});
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
          <PermitApprovalQueue
            items={pendingQueue}
            isBusy={isBusy}
            onApprove={handleApprove}
            onReject={handleReject}
          />
          <PermitCloseQueue items={openPermits} isBusy={isBusy} onClose={handleClose} />
        </>
      )}

      <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <h3 className="mb-4 text-sm font-semibold text-slate-900">Request a work permit</h3>
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
          <h3 className="text-sm font-semibold text-slate-900">My permits</h3>
        </div>
        {myPermits.length === 0 ? (
          <p className="px-6 py-8 text-center text-sm text-slate-500">No permit requests yet.</p>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="text-xs uppercase tracking-wide text-slate-500">
                <th className="px-6 py-2 font-medium">Type</th>
                <th className="px-6 py-2 font-medium">Location</th>
                <th className="px-6 py-2 font-medium">Valid</th>
                <th className="px-6 py-2 font-medium">Status</th>
                <th className="px-6 py-2 font-medium">Submitted</th>
                <th className="px-6 py-2 font-medium">Rejection reason</th>
              </tr>
            </thead>
            <tbody>
              {myPermits.map((permit) => (
                <tr key={permit.id} className="border-t border-slate-100">
                  <td className="px-6 py-2 text-slate-700">{PermitTypeLabels[permit.permitType]}</td>
                  <td className="px-6 py-2 text-slate-700">{permit.location}</td>
                  <td className="px-6 py-2 text-slate-700">
                    {formatDate(permit.validFrom)} – {formatDate(permit.validTo)}
                  </td>
                  <td className="px-6 py-2">
                    <PermitStatusBadge status={permit.status} />
                  </td>
                  <td className="px-6 py-2 text-slate-700">{formatDateTime(permit.createdAtUtc)}</td>
                  <td className="px-6 py-2 text-slate-500">{permit.rejectionReason ?? "—"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
