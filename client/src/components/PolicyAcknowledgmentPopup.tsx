import { useCallback, useEffect, useRef, useState } from "react";
import { getSignature } from "../api/account";
import { extractErrorMessage } from "../api/client";
import { acknowledgePolicy, getPolicies } from "../api/policies";
import { formatDateTime } from "../lib/format";
import { SignaturePad, type SignaturePadHandle } from "./SignaturePad";
import type { PolicyDto } from "../types/policy";

const POLL_INTERVAL_MS = 60_000;
const SNOOZE_MS = 5 * 60_000;

export function PolicyAcknowledgmentPopup() {
  const [queue, setQueue] = useState<PolicyDto[]>([]);
  const [visible, setVisible] = useState(false);
  const [isActing, setIsActing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [hasSavedSignature, setHasSavedSignature] = useState(true);
  const signaturePadRef = useRef<SignaturePadHandle>(null);
  const snoozeTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const queueRef = useRef<PolicyDto[]>([]);

  useEffect(() => {
    queueRef.current = queue;
  }, [queue]);

  const refresh = useCallback(async () => {
    try {
      const policies = await getPolicies();
      const unacknowledged = policies
        .filter((p) => !p.acknowledgedByMe)
        .sort((a, b) => new Date(a.publishedAtUtc).getTime() - new Date(b.publishedAtUtc).getTime());
      setQueue(unacknowledged);
    } catch {
      // Silently ignore — will retry on next poll.
    }
  }, []);

  useEffect(() => {
    refresh();
    const interval = setInterval(refresh, POLL_INTERVAL_MS);
    return () => clearInterval(interval);
  }, [refresh]);

  useEffect(() => {
    getSignature()
      .then((signature) => setHasSavedSignature(signature.hasSignature))
      .catch(() => {
        // Assume a signature exists so the pad doesn't flash in for the common case;
        // the acknowledge call will surface a clear error if one's actually needed.
      });
  }, []);

  useEffect(() => {
    if (queue.length === 0) {
      setVisible(false);
    } else if (!snoozeTimerRef.current) {
      setVisible(true);
    }
  }, [queue]);

  useEffect(() => {
    return () => {
      if (snoozeTimerRef.current) clearTimeout(snoozeTimerRef.current);
    };
  }, []);

  function handleRemindLater() {
    setVisible(false);
    snoozeTimerRef.current = setTimeout(() => {
      snoozeTimerRef.current = null;
      if (queueRef.current.length > 0) setVisible(true);
    }, SNOOZE_MS);
  }

  async function handleAcknowledge() {
    const current = queue[0];
    if (!current) return;

    let signaturePngBase64: string | undefined;
    if (!hasSavedSignature) {
      const drawn = signaturePadRef.current?.getSignature();
      if (!drawn) {
        setError("Please sign above to acknowledge this policy.");
        return;
      }
      signaturePngBase64 = drawn;
    }

    setIsActing(true);
    setError(null);
    try {
      await acknowledgePolicy(current.id, { signaturePngBase64 });
      setHasSavedSignature(true);
      setQueue((prev) => prev.filter((p) => p.id !== current.id));
      if (snoozeTimerRef.current) {
        clearTimeout(snoozeTimerRef.current);
        snoozeTimerRef.current = null;
      }
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsActing(false);
    }
  }

  const current = queue[0];
  if (!visible || !current) return null;

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center bg-slate-950/60 px-4">
      <div className="w-full max-w-lg rounded-lg bg-white p-6 shadow-2xl">
        <p className="text-xs font-semibold uppercase tracking-wide text-yellow-600">New policy</p>
        <h3 className="mt-1 text-lg font-semibold text-slate-900">{current.title}</h3>
        <p className="mt-1 text-xs text-slate-500">
          Published by {current.publishedByName} — {formatDateTime(current.publishedAtUtc)}
        </p>
        <p className="mt-4 max-h-64 overflow-y-auto whitespace-pre-wrap text-sm text-slate-600">
          {current.content}
        </p>
        {queue.length > 1 && (
          <p className="mt-3 text-xs text-slate-400">{queue.length - 1} more polic{queue.length - 1 === 1 ? "y" : "ies"} waiting after this one.</p>
        )}

        {!hasSavedSignature && (
          <div className="mt-4">
            <p className="mb-1 text-xs font-medium text-slate-700">Sign to acknowledge</p>
            <SignaturePad ref={signaturePadRef} height={120} />
          </div>
        )}

        {error && <p className="mt-3 text-sm text-red-600">{error}</p>}
        <div className="mt-5 flex justify-end gap-2">
          <button
            type="button"
            onClick={handleRemindLater}
            disabled={isActing}
            className="rounded-md border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:opacity-50"
          >
            Remind me later
          </button>
          <button
            type="button"
            onClick={handleAcknowledge}
            disabled={isActing}
            className="rounded-md bg-yellow-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-yellow-500 disabled:opacity-50"
          >
            {isActing ? "Acknowledging..." : "Acknowledge"}
          </button>
        </div>
      </div>
    </div>
  );
}
