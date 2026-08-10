import { useCallback, useEffect, useState, type FormEvent } from "react";
import { useAuth } from "../auth/AuthContext";
import { extractErrorMessage } from "../api/client";
import {
  getActiveEmergencyAlerts,
  getMyEmergencyAlerts,
  resolveEmergencyAlert,
  triggerEmergencyAlert,
} from "../api/emergency";
import { EmergencyAlertStatusBadge } from "../components/EmergencyAlertStatusBadge";
import { formatDateTime } from "../lib/format";
import { EmployeeRole } from "../types/auth";
import type { EmergencyAlertDto } from "../types/emergency";

export function EmergencyPage() {
  const { session } = useAuth();
  const isSecurity = session?.role === EmployeeRole.Security;

  const [myAlerts, setMyAlerts] = useState<EmergencyAlertDto[]>([]);
  const [activeAlerts, setActiveAlerts] = useState<EmergencyAlertDto[]>([]);
  const [location, setLocation] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isTriggering, setIsTriggering] = useState(false);
  const [isBusy, setIsBusy] = useState(false);
  const [resolvingId, setResolvingId] = useState<string | null>(null);
  const [resolutionNotes, setResolutionNotes] = useState("");

  const loadAll = useCallback(async () => {
    try {
      const requests: Promise<unknown>[] = [getMyEmergencyAlerts().then(setMyAlerts)];
      if (isSecurity) requests.push(getActiveEmergencyAlerts().then(setActiveAlerts));
      await Promise.all(requests);
    } catch (err) {
      setError(extractErrorMessage(err));
    }
  }, [isSecurity]);

  useEffect(() => {
    loadAll();
  }, [loadAll]);

  async function handleTrigger(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsTriggering(true);
    try {
      await triggerEmergencyAlert({ location, message: message || undefined });
      setLocation("");
      setMessage("");
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsTriggering(false);
    }
  }

  function startResolve(id: string) {
    setResolvingId(id);
    setResolutionNotes("");
  }

  async function confirmResolve(id: string) {
    setIsBusy(true);
    try {
      await resolveEmergencyAlert(id, { resolutionNotes: resolutionNotes || undefined });
      setResolvingId(null);
      setResolutionNotes("");
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsBusy(false);
    }
  }

  return (
    <div className="flex flex-col gap-6">
      {isSecurity && (
        <div className="rounded-lg border border-red-200 bg-white shadow-sm">
          <div className="border-b border-red-100 bg-red-50 px-6 py-4">
            <h3 className="text-sm font-semibold text-red-700">Active SOS alerts</h3>
          </div>
          {activeAlerts.length === 0 ? (
            <p className="px-6 py-8 text-center text-sm text-slate-500">No active alerts.</p>
          ) : (
            <ul className="divide-y divide-slate-100">
              {activeAlerts.map((alert) => (
                <li key={alert.id} className="px-6 py-4">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <p className="text-sm font-medium text-slate-900">{alert.employeeName}</p>
                      <p className="text-sm text-slate-600">
                        {alert.location} — {formatDateTime(alert.triggeredAtUtc)}
                      </p>
                      {alert.message && <p className="mt-1 text-xs text-slate-500">{alert.message}</p>}
                    </div>
                    <button
                      type="button"
                      disabled={isBusy}
                      onClick={() => startResolve(alert.id)}
                      className="rounded-md bg-slate-950 px-3 py-1.5 text-xs font-medium text-white hover:bg-slate-800 disabled:opacity-50"
                    >
                      Resolve
                    </button>
                  </div>

                  {resolvingId === alert.id && (
                    <div className="mt-3 flex flex-wrap items-center gap-2">
                      <input
                        type="text"
                        placeholder="Resolution notes (optional)"
                        value={resolutionNotes}
                        onChange={(e) => setResolutionNotes(e.target.value)}
                        className="min-w-[240px] flex-1 rounded-md border border-slate-300 px-2 py-1 text-xs focus:border-amber-500 focus:outline-none"
                      />
                      <button
                        type="button"
                        disabled={isBusy}
                        onClick={() => confirmResolve(alert.id)}
                        className="rounded-md bg-emerald-600 px-3 py-1 text-xs font-medium text-white hover:bg-emerald-500 disabled:opacity-50"
                      >
                        Confirm resolved
                      </button>
                      <button
                        type="button"
                        onClick={() => setResolvingId(null)}
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
      )}

      <div className="rounded-lg border border-red-200 bg-white p-6 shadow-sm">
        <h3 className="mb-1 text-sm font-semibold text-red-700">Trigger an emergency alert</h3>
        <p className="mb-4 text-xs text-slate-500">
          This will immediately notify Security with your location. Only use for genuine emergencies.
        </p>
        <form onSubmit={handleTrigger} className="flex flex-col gap-3">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Your location
            <input
              required
              value={location}
              onChange={(e) => setLocation(e.target.value)}
              placeholder="e.g. Shaft 3, Level 2"
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-red-500 focus:outline-none"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Message (optional)
            <input
              value={message}
              onChange={(e) => setMessage(e.target.value)}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-red-500 focus:outline-none"
            />
          </label>

          {error && <p className="text-sm text-red-600">{error}</p>}

          <button
            type="submit"
            disabled={isTriggering}
            className="mt-1 w-fit rounded-md bg-red-600 px-6 py-3 text-sm font-semibold text-white hover:bg-red-500 disabled:opacity-50"
          >
            {isTriggering ? "Sending..." : "SOS — Send alert"}
          </button>
        </form>
      </div>

      <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-200 px-6 py-4">
          <h3 className="text-sm font-semibold text-slate-900">My alerts</h3>
        </div>
        {myAlerts.length === 0 ? (
          <p className="px-6 py-8 text-center text-sm text-slate-500">No alerts triggered yet.</p>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="text-xs uppercase tracking-wide text-slate-500">
                <th className="px-6 py-2 font-medium">Location</th>
                <th className="px-6 py-2 font-medium">Triggered</th>
                <th className="px-6 py-2 font-medium">Status</th>
                <th className="px-6 py-2 font-medium">Resolved</th>
                <th className="px-6 py-2 font-medium">Resolution notes</th>
              </tr>
            </thead>
            <tbody>
              {myAlerts.map((alert) => (
                <tr key={alert.id} className="border-t border-slate-100">
                  <td className="px-6 py-2 text-slate-700">{alert.location}</td>
                  <td className="px-6 py-2 text-slate-700">{formatDateTime(alert.triggeredAtUtc)}</td>
                  <td className="px-6 py-2">
                    <EmergencyAlertStatusBadge status={alert.status} />
                  </td>
                  <td className="px-6 py-2 text-slate-700">
                    {alert.resolvedAtUtc ? formatDateTime(alert.resolvedAtUtc) : "—"}
                  </td>
                  <td className="px-6 py-2 text-slate-500">{alert.resolutionNotes ?? "—"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
