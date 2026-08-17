import { useCallback, useEffect, useRef, useState, type FormEvent } from "react";
import { useAuth } from "../auth/AuthContext";
import { getSignature } from "../api/account";
import { extractErrorMessage } from "../api/client";
import {
  acknowledgePolicy,
  createPolicy,
  downloadSignedPolicyAttachment,
  getPolicies,
  getPolicyAcknowledgments,
} from "../api/policies";
import { getAttachmentsForEntity, uploadAttachment } from "../api/attachments";
import { AttachmentsPanel } from "../components/AttachmentsPanel";
import { Badge } from "../components/Badge";
import { SignaturePad, type SignaturePadHandle } from "../components/SignaturePad";
import { formatDateTime } from "../lib/format";
import { AttachmentEntityType, type AttachmentDto } from "../types/attachment";
import { EmployeeRole } from "../types/auth";
import type { PolicyAcknowledgmentDto, PolicyDto } from "../types/policy";

export function PoliciesPage() {
  const { session } = useAuth();
  const isHR = session?.role === EmployeeRole.HR;

  const [policies, setPolicies] = useState<PolicyDto[]>([]);
  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");
  const [attachmentFile, setAttachmentFile] = useState<File | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [isActing, setIsActing] = useState(false);
  const [openRosterId, setOpenRosterId] = useState<string | null>(null);
  const [roster, setRoster] = useState<PolicyAcknowledgmentDto[]>([]);
  const [rosterAttachments, setRosterAttachments] = useState<AttachmentDto[]>([]);
  const [hasSavedSignature, setHasSavedSignature] = useState(true);
  const [signingId, setSigningId] = useState<string | null>(null);
  const signaturePadRef = useRef<SignaturePadHandle>(null);

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

  useEffect(() => {
    getSignature()
      .then((signature) => setHasSavedSignature(signature.hasSignature))
      .catch(() => {
        // Assume a signature exists so the inline pad doesn't flash in unnecessarily.
      });
  }, []);

  async function handlePublish(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      const policy = await createPolicy({ title, content });
      if (attachmentFile) {
        try {
          await uploadAttachment(AttachmentEntityType.Policy, policy.id, attachmentFile);
        } catch (err) {
          setError(
            `The policy was published, but the attachment failed to upload: ${extractErrorMessage(err)}. Attach it below.`,
          );
          await loadPolicies();
          return;
        }
      }
      setTitle("");
      setContent("");
      setAttachmentFile(null);
      if (fileInputRef.current) fileInputRef.current.value = "";
      await loadPolicies();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  function handleAcknowledgeClick(id: string) {
    if (!hasSavedSignature) {
      setSigningId(id);
      return;
    }
    void acknowledge(id);
  }

  async function acknowledge(id: string, signaturePngBase64?: string) {
    setIsActing(true);
    setError(null);
    try {
      await acknowledgePolicy(id, { signaturePngBase64 });
      setHasSavedSignature(true);
      setSigningId(null);
      await loadPolicies();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsActing(false);
    }
  }

  function confirmSign(id: string) {
    const drawn = signaturePadRef.current?.getSignature();
    if (!drawn) {
      setError("Please sign to acknowledge this policy.");
      return;
    }
    void acknowledge(id, drawn);
  }

  async function toggleRoster(id: string) {
    if (openRosterId === id) {
      setOpenRosterId(null);
      return;
    }
    setIsActing(true);
    try {
      const [ackData, attachments] = await Promise.all([
        getPolicyAcknowledgments(id),
        getAttachmentsForEntity(AttachmentEntityType.Policy, id),
      ]);
      setRoster(ackData);
      setRosterAttachments(attachments);
      setOpenRosterId(id);
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsActing(false);
    }
  }

  async function handleDownloadSigned(policyId: string, employeeId: string, employeeName: string, attachment: AttachmentDto) {
    try {
      await downloadSignedPolicyAttachment(
        policyId, employeeId, attachment.id, `${attachment.fileName}-signed-${employeeName}.pdf`,
      );
    } catch (err) {
      setError(extractErrorMessage(err));
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
                className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
              />
            </label>
            <label className="flex flex-col gap-1 text-sm text-slate-700">
              Content
              <textarea
                required
                value={content}
                onChange={(e) => setContent(e.target.value)}
                rows={4}
                className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
              />
            </label>
            <label className="flex flex-col gap-1 text-sm text-slate-700">
              Attachment (optional)
              <input
                ref={fileInputRef}
                type="file"
                accept="application/pdf,image/jpeg,image/png"
                onChange={(e) => setAttachmentFile(e.target.files?.[0] ?? null)}
                className="rounded-md border border-slate-300 px-3 py-1.5 text-sm file:mr-2 file:rounded file:border-0 file:bg-slate-100 file:px-2 file:py-1 file:text-xs file:font-medium file:text-slate-700 focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
              />
            </label>
            {error && <p className="text-sm text-red-600">{error}</p>}
            <button
              type="submit"
              disabled={isSubmitting}
              className="mt-1 w-fit rounded-md bg-yellow-600 px-4 py-2 text-sm font-medium text-white hover:bg-yellow-500 disabled:opacity-50"
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
                        onClick={() => handleAcknowledgeClick(policy.id)}
                        className="rounded-md bg-yellow-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-yellow-500 disabled:opacity-50"
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

                {signingId === policy.id && (
                  <div className="mt-3 max-w-sm rounded-md border border-slate-200 bg-slate-50 p-3">
                    <p className="mb-1 text-xs font-medium text-slate-700">Sign to acknowledge</p>
                    <SignaturePad ref={signaturePadRef} height={110} />
                    <div className="mt-2 flex gap-2">
                      <button
                        type="button"
                        disabled={isActing}
                        onClick={() => confirmSign(policy.id)}
                        className="rounded-md bg-yellow-600 px-3 py-1 text-xs font-medium text-white hover:bg-yellow-500 disabled:opacity-50"
                      >
                        Confirm & acknowledge
                      </button>
                      <button
                        type="button"
                        onClick={() => setSigningId(null)}
                        className="text-xs text-slate-500 hover:underline"
                      >
                        Cancel
                      </button>
                    </div>
                  </div>
                )}

                {isHR && openRosterId === policy.id && (
                  <div className="mt-3 rounded-md bg-slate-50 p-3">
                    {roster.length === 0 ? (
                      <p className="text-xs text-slate-500">No one has acknowledged this yet.</p>
                    ) : (
                      <ul className="flex flex-col gap-2">
                        {roster.map((entry) => (
                          <li key={entry.employeeId} className="text-xs text-slate-600">
                            <span>
                              {entry.employeeName} — {formatDateTime(entry.acknowledgedAtUtc)}
                            </span>
                            {rosterAttachments.length > 0 && (
                              <span className="ml-2 inline-flex flex-wrap gap-2">
                                {rosterAttachments.map((attachment) => (
                                  <button
                                    key={attachment.id}
                                    type="button"
                                    onClick={() => handleDownloadSigned(policy.id, entry.employeeId, entry.employeeName, attachment)}
                                    className="font-medium text-yellow-700 hover:underline"
                                  >
                                    Download signed: {attachment.fileName}
                                  </button>
                                ))}
                              </span>
                            )}
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
