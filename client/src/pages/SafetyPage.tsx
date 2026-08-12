import { useCallback, useEffect, useState, type FormEvent } from "react";
import { useAuth } from "../auth/AuthContext";
import { extractErrorMessage } from "../api/client";
import {
  getPreShiftCheckHistory,
  getTodaysHazards,
  getTodaysPreShiftCheck,
  submitPreShiftCheck,
} from "../api/safety";
import { Badge } from "../components/Badge";
import { formatDate, formatDateTime } from "../lib/format";
import { EmployeeRole } from "../types/auth";
import type { PreShiftSafetyCheck } from "../types/safety";

export function SafetyPage() {
  const { session } = useAuth();
  const isSafetyOfficer = session?.role === EmployeeRole.SafetyOfficer;

  const [today, setToday] = useState<PreShiftSafetyCheck | null | undefined>(undefined);
  const [history, setHistory] = useState<PreShiftSafetyCheck[]>([]);
  const [hazardsToday, setHazardsToday] = useState<PreShiftSafetyCheck[]>([]);
  const [hazardsIdentified, setHazardsIdentified] = useState<"yes" | "no">("no");
  const [notes, setNotes] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const loadAll = useCallback(async () => {
    try {
      const requests: Promise<unknown>[] = [
        getTodaysPreShiftCheck().then(setToday),
        getPreShiftCheckHistory().then(setHistory),
      ];
      if (isSafetyOfficer) requests.push(getTodaysHazards().then(setHazardsToday));
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
      await submitPreShiftCheck({
        hazardsIdentified: hazardsIdentified === "yes",
        hazardNotes: notes || undefined,
      });
      setNotes("");
      await loadAll();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="stagger-children flex flex-col gap-6">
      {isSafetyOfficer && (
        <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
          <div className="border-b border-slate-200 px-6 py-4">
            <h3 className="text-sm font-semibold text-slate-900">Today's flagged hazards</h3>
          </div>
          {hazardsToday.length === 0 ? (
            <p className="px-6 py-8 text-center text-sm text-slate-500">No hazards flagged today.</p>
          ) : (
            <ul className="divide-y divide-slate-100">
              {hazardsToday.map((item) => (
                <li key={item.id} className="px-6 py-4">
                  <p className="text-sm font-medium text-slate-900">{item.employeeName}</p>
                  <p className="mt-1 text-xs text-slate-500">
                    {item.hazardNotes || "No details provided"} — {formatDateTime(item.submittedAtUtc)}
                  </p>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <h3 className="mb-4 text-sm font-semibold text-slate-900">Pre-shift safety check</h3>
        {today === undefined ? (
          <p className="text-sm text-slate-500">Loading...</p>
        ) : today ? (
          <div>
            <p className="text-sm text-slate-700">You've completed today's check.</p>
            <div className="mt-2">
              <Badge
                label={today.hazardsIdentified ? "Hazards identified" : "No hazards identified"}
                tone={today.hazardsIdentified ? "amber" : "emerald"}
              />
            </div>
            {today.hazardNotes && <p className="mt-2 text-sm text-slate-600">Notes: {today.hazardNotes}</p>}
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="flex flex-col gap-3">
            <p className="text-sm text-slate-700">Have you identified any new hazards in your work area?</p>
            <div className="flex gap-4">
              <label className="flex items-center gap-2 text-sm text-slate-700">
                <input
                  type="radio"
                  name="hazards"
                  checked={hazardsIdentified === "no"}
                  onChange={() => setHazardsIdentified("no")}
                />
                No
              </label>
              <label className="flex items-center gap-2 text-sm text-slate-700">
                <input
                  type="radio"
                  name="hazards"
                  checked={hazardsIdentified === "yes"}
                  onChange={() => setHazardsIdentified("yes")}
                />
                Yes
              </label>
            </div>

            <label className="flex flex-col gap-1 text-sm text-slate-700">
              Notes (optional)
              <textarea
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                rows={2}
                className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
              />
            </label>

            {error && <p className="text-sm text-red-600">{error}</p>}

            <button
              type="submit"
              disabled={isSubmitting}
              className="mt-1 w-fit rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-500 disabled:opacity-50"
            >
              {isSubmitting ? "Submitting..." : "Submit check"}
            </button>
          </form>
        )}
      </div>

      <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-200 px-6 py-4">
          <h3 className="text-sm font-semibold text-slate-900">History</h3>
        </div>
        {history.length === 0 ? (
          <p className="px-6 py-8 text-center text-sm text-slate-500">No pre-shift checks yet.</p>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="text-xs uppercase tracking-wide text-slate-500">
                <th className="px-6 py-2 font-medium">Date</th>
                <th className="px-6 py-2 font-medium">Result</th>
                <th className="px-6 py-2 font-medium">Notes</th>
                <th className="px-6 py-2 font-medium">Submitted</th>
              </tr>
            </thead>
            <tbody>
              {history.map((item) => (
                <tr key={item.id} className="border-t border-slate-100">
                  <td className="px-6 py-2 text-slate-700">{formatDate(item.checkDate)}</td>
                  <td className="px-6 py-2">
                    <Badge
                      label={item.hazardsIdentified ? "Hazards identified" : "No hazards"}
                      tone={item.hazardsIdentified ? "amber" : "emerald"}
                    />
                  </td>
                  <td className="px-6 py-2 text-slate-500">{item.hazardNotes ?? "—"}</td>
                  <td className="px-6 py-2 text-slate-700">{formatDateTime(item.submittedAtUtc)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
