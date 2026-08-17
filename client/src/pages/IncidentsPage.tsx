import { useCallback, useEffect, useRef, useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { uploadAttachment } from "../api/attachments";
import { extractErrorMessage } from "../api/client";
import {
  getAllIncidentReports,
  getMyIncidentReports,
  submitIncidentReport,
  updateIncidentStatus,
} from "../api/incidents";
import { AcknowledgmentPanel } from "../components/AcknowledgmentPanel";
import { AttachmentsPanel } from "../components/AttachmentsPanel";
import { IncidentSeverityBadge, IncidentStatusBadge } from "../components/IncidentBadges";
import { StepForm, type WizardStep } from "../components/StepForm";
import { formatDateTime } from "../lib/format";
import { AcknowledgmentEntityType } from "../types/acknowledgment";
import { AttachmentEntityType } from "../types/attachment";
import { EmployeeRole } from "../types/auth";
import {
  IncidentSeverity,
  IncidentSeverityLabels,
  IncidentStatus,
  type IncidentReportDto,
} from "../types/incident";

interface IncidentForm {
  title: string;
  description: string;
  severity: IncidentSeverity;
  location: string;
  occurredAt: string;
}

const initialForm: IncidentForm = {
  title: "",
  description: "",
  severity: IncidentSeverity.Low,
  location: "",
  occurredAt: "",
};

export function IncidentsPage() {
  const { session } = useAuth();
  const isSafetyOfficer = session?.role === EmployeeRole.SafetyOfficer;
  const isHR = session?.role === EmployeeRole.HR;
  const isExecutive = session?.role === EmployeeRole.Executive;
  const canViewAllReports = isSafetyOfficer || isHR || isExecutive;

  const [myReports, setMyReports] = useState<IncidentReportDto[]>([]);
  const [allReports, setAllReports] = useState<IncidentReportDto[]>([]);
  const [form, setForm] = useState(initialForm);
  const [attachmentFile, setAttachmentFile] = useState<File | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isUpdating, setIsUpdating] = useState(false);
  const [notesById, setNotesById] = useState<Record<string, string>>({});
  const fileInputRef = useRef<HTMLInputElement>(null);

  const loadAll = useCallback(async () => {
    try {
      const requests: Promise<unknown>[] = [getMyIncidentReports().then(setMyReports)];
      if (canViewAllReports) requests.push(getAllIncidentReports().then(setAllReports));
      await Promise.all(requests);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  }, [canViewAllReports]);

  useEffect(() => {
    loadAll();
  }, [loadAll]);

  async function handleSubmit() {
    setError(null);
    setIsSubmitting(true);
    try {
      const report = await submitIncidentReport({
        title: form.title,
        description: form.description,
        severity: form.severity,
        location: form.location,
        occurredAtUtc: new Date(form.occurredAt).toISOString(),
      });
      if (attachmentFile) {
        try {
          await uploadAttachment(AttachmentEntityType.IncidentReport, report.id, attachmentFile);
        } catch (err) {
          setError(
            `The report was submitted, but the attachment failed to upload: ${extractErrorMessage(err)}. Attach it below.`,
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
      title: "What happened",
      validate: () => (!form.title || !form.location ? "Please fill in all fields." : null),
      content: (
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm text-slate-700 sm:col-span-2">
            Title
            <input
              required
              value={form.title}
              onChange={(e) => setForm((prev) => ({ ...prev, title: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Severity
            <select
              value={form.severity}
              onChange={(e) => setForm((prev) => ({ ...prev, severity: Number(e.target.value) as IncidentSeverity }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            >
              {Object.entries(IncidentSeverityLabels).map(([value, label]) => (
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
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>
        </div>
      ),
    },
    {
      title: "Details",
      validate: () => (!form.occurredAt || !form.description ? "Please fill in all fields." : null),
      content: (
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm text-slate-700 sm:col-span-2">
            When did it happen?
            <input
              type="datetime-local"
              required
              value={form.occurredAt}
              onChange={(e) => setForm((prev) => ({ ...prev, occurredAt: e.target.value }))}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-slate-700 sm:col-span-2">
            Description
            <textarea
              required
              value={form.description}
              onChange={(e) => setForm((prev) => ({ ...prev, description: e.target.value }))}
              rows={3}
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

  async function advanceStatus(id: string, status: IncidentStatus) {
    setIsUpdating(true);
    try {
      await updateIncidentStatus(id, { status, reviewNotes: notesById[id] || undefined });
      setNotesById((prev) => ({ ...prev, [id]: "" }));
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsUpdating(false);
    }
  }

  return (
    <div className="stagger-children flex flex-col gap-6">
      {canViewAllReports && (
        <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
          <div className="border-b border-slate-200 px-6 py-4">
            <h3 className="text-sm font-semibold text-slate-900">All incident reports</h3>
          </div>
          {allReports.length === 0 ? (
            <p className="px-6 py-8 text-center text-sm text-slate-500">No incident reports yet.</p>
          ) : (
            <ul className="divide-y divide-slate-100">
              {allReports.map((item) => (
                <li key={item.id} className="px-6 py-4">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <p className="text-sm font-medium text-slate-900">{item.title}</p>
                      <p className="text-xs text-slate-500">
                        {item.employeeName} — {item.location} — {formatDateTime(item.occurredAtUtc)}
                      </p>
                      <p className="mt-1 text-sm text-slate-600">{item.description}</p>
                      <div className="mt-2 flex gap-2">
                        <IncidentSeverityBadge severity={item.severity} />
                        <IncidentStatusBadge status={item.status} />
                      </div>
                      {item.reviewNotes && (
                        <p className="mt-2 text-xs text-slate-500">Review notes: {item.reviewNotes}</p>
                      )}
                    </div>
                    {isSafetyOfficer && (
                      <div className="flex flex-col items-end gap-2">
                        {item.status === IncidentStatus.Reported && (
                          <button
                            type="button"
                            disabled={isUpdating}
                            onClick={() => advanceStatus(item.id, IncidentStatus.UnderInvestigation)}
                            className="rounded-md bg-red-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-red-500 disabled:opacity-50"
                          >
                            Start investigation
                          </button>
                        )}
                        {item.status === IncidentStatus.UnderInvestigation && (
                          <div className="flex flex-col items-end gap-1">
                            <input
                              type="text"
                              placeholder="Resolution notes (optional)"
                              value={notesById[item.id] ?? ""}
                              onChange={(e) =>
                                setNotesById((prev) => ({ ...prev, [item.id]: e.target.value }))
                              }
                              className="w-56 rounded-md border border-slate-300 px-2 py-1 text-xs focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
                            />
                            <button
                              type="button"
                              disabled={isUpdating}
                              onClick={() => advanceStatus(item.id, IncidentStatus.Closed)}
                              className="rounded-md bg-emerald-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-emerald-500 disabled:opacity-50"
                            >
                              Close incident
                            </button>
                          </div>
                        )}
                      </div>
                    )}
                  </div>
                  <AttachmentsPanel entityType={AttachmentEntityType.IncidentReport} entityId={item.id} canUpload />
                  <AcknowledgmentPanel entityType={AcknowledgmentEntityType.IncidentReport} entityId={item.id} />
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <h3 className="mb-4 text-sm font-semibold text-slate-900">Report an incident or near miss</h3>
        <StepForm
          steps={steps}
          onSubmit={handleSubmit}
          submitLabel="Submit report"
          submittingLabel="Submitting..."
          isSubmitting={isSubmitting}
          error={error}
        />
      </div>

      <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-200 px-6 py-4">
          <h3 className="text-sm font-semibold text-slate-900">My reports</h3>
        </div>
        {myReports.length === 0 ? (
          <p className="px-6 py-8 text-center text-sm text-slate-500">No incident reports yet.</p>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="text-xs uppercase tracking-wide text-slate-500">
                <th className="px-6 py-2 font-medium">Title</th>
                <th className="px-6 py-2 font-medium">Severity</th>
                <th className="px-6 py-2 font-medium">Status</th>
                <th className="px-6 py-2 font-medium">Occurred</th>
                <th className="px-6 py-2 font-medium">Review notes</th>
                <th className="px-6 py-2 font-medium">Attachments</th>
              </tr>
            </thead>
            <tbody>
              {myReports.map((item) => (
                <tr key={item.id} className="border-t border-slate-100">
                  <td className="px-6 py-2 text-slate-700">{item.title}</td>
                  <td className="px-6 py-2">
                    <IncidentSeverityBadge severity={item.severity} />
                  </td>
                  <td className="px-6 py-2">
                    <IncidentStatusBadge status={item.status} />
                  </td>
                  <td className="px-6 py-2 text-slate-700">{formatDateTime(item.occurredAtUtc)}</td>
                  <td className="px-6 py-2 text-slate-500">{item.reviewNotes ?? "—"}</td>
                  <td className="px-6 py-2">
                    <AttachmentsPanel
                      entityType={AttachmentEntityType.IncidentReport}
                      entityId={item.id}
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
    </div>
  );
}
