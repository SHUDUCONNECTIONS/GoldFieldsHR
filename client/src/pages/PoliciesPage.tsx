import { useCallback, useEffect, useState, type FormEvent } from "react";
import { useAuth } from "../auth/AuthContext";
import { extractErrorMessage } from "../api/client";
import { acknowledgePolicy, createPolicy, getPolicies, getPolicyAcknowledgments } from "../api/policies";
import { AttachmentsPanel } from "../components/AttachmentsPanel";
import { Badge } from "../components/Badge";
import { formatDateTime } from "../lib/format";
import { AttachmentEntityType } from "../types/attachment";
import { EmployeeRole } from "../types/auth";
import type { PolicyAcknowledgmentDto, PolicyDto } from "../types/policy";

export function PoliciesPage() {
  const { session } = useAuth();
  const isHR = session?.role === EmployeeRole.HR;

  const [policies, setPolicies] = useState<PolicyDto[]>([]);
  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isActing, setIsActing] = useState(false);
  const [openRosterId, setOpenRosterId] = useState<string | null>(null);
  const [roster, setRoster] = useState<PolicyAcknowledgmentDto[]>([]);

  const loadPolicies = useCallback(async () => {
    try {
      const data = await getPolicies();
      setPolicies(data);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  }, []);

  useEffect(() => {
    loadPolicies();
  }, [loadPolicies]);

  async function handlePublish(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      await createPolicy({ title, content });
      setTitle("");
      setContent("");
      await loadPolicies();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleAcknowledge(id: string) {
    setIsActing(true);
    try {
      await acknowledgePolicy(id);
      await loadPolicies();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsActing(false);
    }
  }

  async function toggleRoster(id: string) {
    if (openRosterId === id) {
      setOpenRosterId(null);
      return;
    }
    setIsActing(true);
    try {
      const data = await getPolicyAcknowledgments(id);
      setRoster(data);
      setOpenRosterId(id);
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsActing(false);
    }
  }

  return (
    <div className="stagger-children flex flex-col gap-6">
      {isHR && (
        <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
          <h3 className="mb-4 text-sm font-semibold text-slate-900">Publish a policy</h3>
          <form onSubmit={handlePublish} className="flex flex-col gap-3">
            <label className="flex flex-col gap-1 text-sm text-slate-700">
              Title
              <input
                required
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
              />
            </label>
            <label className="flex flex-col gap-1 text-sm text-slate-700">
              Content
              <textarea
                required
                value={content}
                onChange={(e) => setContent(e.target.value)}
                rows={4}
                className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-amber-500 focus:outline-none"
              />
            </label>
            {error && <p className="text-sm text-red-600">{error}</p>}
            <button
              type="submit"
              disabled={isSubmitting}
              className="mt-1 w-fit rounded-md bg-slate-950 px-4 py-2 text-sm font-medium text-white hover:bg-slate-800 disabled:opacity-50"
            >
              {isSubmitting ? "Publishing..." : "Publish"}
            </button>
          </form>
        </div>
      )}

      <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-200 px-6 py-4">
          <h3 className="text-sm font-semibold text-slate-900">Policies & documents</h3>
        </div>
        {policies.length === 0 ? (
          <p className="px-6 py-8 text-center text-sm text-slate-500">No policies published yet.</p>
        ) : (
          <ul className="divide-y divide-slate-100">
            {policies.map((policy) => (
              <li key={policy.id} className="px-6 py-4">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div className="max-w-2xl">
                    <p className="text-sm font-medium text-slate-900">{policy.title}</p>
                    <p className="text-xs text-slate-500">
                      Published by {policy.publishedByName} — {formatDateTime(policy.publishedAtUtc)}
                    </p>
                    <p className="mt-2 whitespace-pre-wrap text-sm text-slate-600">{policy.content}</p>
                  </div>
                  <div className="flex flex-col items-end gap-2">
                    {policy.acknowledgedByMe ? (
                      <Badge label={`Acknowledged ${formatDateTime(policy.acknowledgedAtUtc!)}`} tone="emerald" />
                    ) : (
                      <button
                        type="button"
                        disabled={isActing}
                        onClick={() => handleAcknowledge(policy.id)}
                        className="rounded-md bg-amber-500 px-3 py-1.5 text-xs font-medium text-slate-950 hover:bg-amber-400 disabled:opacity-50"
                      >
                        Acknowledge
                      </button>
                    )}
                    {isHR && (
                      <button
                        type="button"
                        onClick={() => toggleRoster(policy.id)}
                        className="text-xs text-slate-500 hover:underline"
                      >
                        {policy.acknowledgmentCount} acknowledged — view roster
                      </button>
                    )}
                  </div>
                </div>

                {isHR && openRosterId === policy.id && (
                  <div className="mt-3 rounded-md bg-slate-50 p-3">
                    {roster.length === 0 ? (
                      <p className="text-xs text-slate-500">No one has acknowledged this yet.</p>
                    ) : (
                      <ul className="flex flex-col gap-1">
                        {roster.map((entry) => (
                          <li key={entry.employeeId} className="text-xs text-slate-600">
                            {entry.employeeName} — {formatDateTime(entry.acknowledgedAtUtc)}
                          </li>
                        ))}
                      </ul>
                    )}
                  </div>
                )}

                <AttachmentsPanel entityType={AttachmentEntityType.Policy} entityId={policy.id} canUpload={isHR} />
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
