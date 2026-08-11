import { Fragment, useCallback, useEffect, useState } from "react";
import {
  clockIn,
  clockOut,
  getHistory,
  getMyTimesheetCorrections,
  getPendingTimesheetCorrections,
  getToday,
  reviewTimesheetCorrection,
  submitTimesheetCorrection,
} from "../api/timesheet";
import { extractErrorMessage } from "../api/client";
import { Badge } from "../components/Badge";
import { useAuth } from "../auth/AuthContext";
import { formatDateTime, formatDuration, formatTime } from "../lib/format";
import { EmployeeRole } from "../types/auth";
import {
  TimesheetCorrectionStatus,
  TimesheetCorrectionStatusLabels,
  type TimesheetCorrectionDto,
  type TimesheetEntry,
} from "../types/timesheet";

function toDateTimeLocalInputValue(iso: string): string {
  const date = new Date(iso);
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

export function TimesheetPage() {
  const { session } = useAuth();
  const isLineManager = session?.role === EmployeeRole.LineManager;

  const [openEntry, setOpenEntry] = useState<TimesheetEntry | null>(null);
  const [history, setHistory] = useState<TimesheetEntry[]>([]);
  const [myCorrections, setMyCorrections] = useState<TimesheetCorrectionDto[]>([]);
  const [pendingCorrections, setPendingCorrections] = useState<TimesheetCorrectionDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isReviewing, setIsReviewing] = useState(false);

  const [correctingEntryId, setCorrectingEntryId] = useState<string | null>(null);
  const [correctedClockIn, setCorrectedClockIn] = useState("");
  const [correctedClockOut, setCorrectedClockOut] = useState("");
  const [correctionReason, setCorrectionReason] = useState("");

  const loadData = useCallback(async () => {
    setIsLoading(true);
    try {
      const requests: Promise<unknown>[] = [
        getToday().then(setOpenEntry),
        getHistory().then(setHistory),
        getMyTimesheetCorrections().then(setMyCorrections),
      ];
      if (isLineManager) requests.push(getPendingTimesheetCorrections().then(setPendingCorrections));
      await Promise.all(requests);
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsLoading(false);
    }
  }, [isLineManager]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  async function handleClockIn() {
    setError(null);
    setIsSubmitting(true);
    try {
      await clockIn();
      await loadData();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleClockOut() {
    setError(null);
    setIsSubmitting(true);
    try {
      await clockOut();
      await loadData();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  function startCorrection(entry: TimesheetEntry) {
    setCorrectingEntryId(entry.id);
    setCorrectedClockIn(toDateTimeLocalInputValue(entry.clockInUtc));
    setCorrectedClockOut(entry.clockOutUtc ? toDateTimeLocalInputValue(entry.clockOutUtc) : "");
    setCorrectionReason("");
  }

  async function submitCorrection(entry: TimesheetEntry) {
    setIsSubmitting(true);
    setError(null);
    try {
      await submitTimesheetCorrection({
        timesheetEntryId: entry.id,
        requestedClockInUtc: new Date(correctedClockIn).toISOString(),
        requestedClockOutUtc: correctedClockOut ? new Date(correctedClockOut).toISOString() : undefined,
        reason: correctionReason,
      });
      setCorrectingEntryId(null);
      await loadData();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleReview(id: string, approve: boolean) {
    setIsReviewing(true);
    setError(null);
    try {
      await reviewTimesheetCorrection(id, { approve });
      await loadData();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setIsReviewing(false);
    }
  }

  return (
    <div className="stagger-children flex flex-col gap-6">
      {isLineManager && (
        <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
          <div className="border-b border-slate-200 px-6 py-4">
            <h3 className="text-sm font-semibold text-slate-900">Pending correction approvals</h3>
          </div>
          {pendingCorrections.length === 0 ? (
            <p className="px-6 py-8 text-center text-sm text-slate-500">Nothing pending approval.</p>
          ) : (
            <ul className="divide-y divide-slate-100">
              {pendingCorrections.map((c) => (
                <li key={c.id} className="flex flex-wrap items-start justify-between gap-3 px-6 py-4">
                  <div>
                    <p className="flex items-center gap-2 text-sm font-medium text-slate-900">
                      {c.employeeName}
                      {c.isDirectReport && (
                        <span className="rounded-full bg-amber-100 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-amber-700">
                          My team
                        </span>
                      )}
                    </p>
                    <p className="text-sm text-slate-600">
                      Original: {formatDateTime(c.originalClockInUtc)} –{" "}
                      {c.originalClockOutUtc ? formatDateTime(c.originalClockOutUtc) : "in progress"}
                    </p>
                    <p className="text-sm text-slate-600">
                      Requested: {c.requestedClockInUtc ? formatDateTime(c.requestedClockInUtc) : "(no change)"} –{" "}
                      {c.requestedClockOutUtc ? formatDateTime(c.requestedClockOutUtc) : "(no change)"}
                    </p>
                    <p className="mt-1 text-xs text-slate-500">Reason: {c.reason}</p>
                  </div>
                  <div className="flex gap-2">
                    <button
                      type="button"
                      disabled={isReviewing}
                      onClick={() => handleReview(c.id, true)}
                      className="rounded-md bg-emerald-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-emerald-500 disabled:opacity-50"
                    >
                      Approve
                    </button>
                    <button
                      type="button"
                      disabled={isReviewing}
                      onClick={() => handleReview(c.id, false)}
                      className="rounded-md bg-red-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-red-500 disabled:opacity-50"
                    >
                      Reject
                    </button>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        {isLoading ? (
          <p className="text-sm text-slate-500">Loading...</p>
        ) : openEntry ? (
          <>
            <p className="text-xs font-medium uppercase tracking-wide text-slate-500">
              Currently clocked in
            </p>
            <p className="mt-1 text-lg font-semibold text-slate-900">
              Since {formatTime(openEntry.clockInUtc)}
            </p>
            <button
              type="button"
              onClick={handleClockOut}
              disabled={isSubmitting}
              className="mt-4 rounded-md bg-slate-950 px-4 py-2 text-sm font-medium text-white hover:bg-slate-800 disabled:opacity-50"
            >
              {isSubmitting ? "Clocking out..." : "Clock out"}
            </button>
          </>
        ) : (
          <>
            <p className="text-xs font-medium uppercase tracking-wide text-slate-500">Status</p>
            <p className="mt-1 text-lg font-semibold text-slate-900">Not clocked in</p>
            <button
              type="button"
              onClick={handleClockIn}
              disabled={isSubmitting}
              className="mt-4 rounded-md bg-amber-500 px-4 py-2 text-sm font-medium text-slate-950 hover:bg-amber-400 disabled:opacity-50"
            >
              {isSubmitting ? "Clocking in..." : "Clock in"}
            </button>
          </>
        )}

        {error && <p className="mt-3 text-sm text-red-600">{error}</p>}
      </div>

      <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-200 px-6 py-4">
          <h3 className="text-sm font-semibold text-slate-900">History</h3>
        </div>
        {history.length === 0 && !isLoading ? (
          <p className="px-6 py-8 text-center text-sm text-slate-500">No timesheet entries yet.</p>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="text-xs uppercase tracking-wide text-slate-500">
                <th className="px-6 py-2 font-medium">Clock in</th>
                <th className="px-6 py-2 font-medium">Clock out</th>
                <th className="px-6 py-2 font-medium">Duration</th>
                <th className="px-6 py-2 font-medium">Action</th>
              </tr>
            </thead>
            <tbody>
              {history.map((entry) => (
                <Fragment key={entry.id}>
                  <tr className="border-t border-slate-100">
                    <td className="px-6 py-2 text-slate-700">{formatDateTime(entry.clockInUtc)}</td>
                    <td className="px-6 py-2 text-slate-700">
                      {entry.clockOutUtc ? formatDateTime(entry.clockOutUtc) : "In progress"}
                    </td>
                    <td className="px-6 py-2 text-slate-700">{formatDuration(entry.durationHours)}</td>
                    <td className="px-6 py-2">
                      {correctingEntryId !== entry.id && (
                        <button
                          type="button"
                          onClick={() => startCorrection(entry)}
                          className="text-xs text-amber-600 hover:underline"
                        >
                          Request correction
                        </button>
                      )}
                    </td>
                  </tr>
                  {correctingEntryId === entry.id && (
                    <tr className="border-t border-slate-100 bg-slate-50">
                      <td colSpan={4} className="px-6 py-4">
                        <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
                          <label className="flex flex-col gap-1 text-xs text-slate-700">
                            Corrected clock in
                            <input
                              type="datetime-local"
                              value={correctedClockIn}
                              onChange={(e) => setCorrectedClockIn(e.target.value)}
                              className="rounded-md border border-slate-300 px-2 py-1.5 text-xs focus:border-amber-500 focus:outline-none"
                            />
                          </label>
                          <label className="flex flex-col gap-1 text-xs text-slate-700">
                            Corrected clock out
                            <input
                              type="datetime-local"
                              value={correctedClockOut}
                              onChange={(e) => setCorrectedClockOut(e.target.value)}
                              className="rounded-md border border-slate-300 px-2 py-1.5 text-xs focus:border-amber-500 focus:outline-none"
                            />
                          </label>
                          <label className="flex flex-col gap-1 text-xs text-slate-700">
                            Reason
                            <input
                              required
                              value={correctionReason}
                              onChange={(e) => setCorrectionReason(e.target.value)}
                              className="rounded-md border border-slate-300 px-2 py-1.5 text-xs focus:border-amber-500 focus:outline-none"
                            />
                          </label>
                        </div>
                        <div className="mt-3 flex gap-2">
                          <button
                            type="button"
                            disabled={isSubmitting || !correctionReason}
                            onClick={() => submitCorrection(entry)}
                            className="rounded-md bg-slate-950 px-3 py-1.5 text-xs font-medium text-white hover:bg-slate-800 disabled:opacity-50"
                          >
                            Submit request
                          </button>
                          <button
                            type="button"
                            onClick={() => setCorrectingEntryId(null)}
                            className="text-xs text-slate-500 hover:underline"
                          >
                            Cancel
                          </button>
                        </div>
                      </td>
                    </tr>
                  )}
                </Fragment>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <div className="rounded-lg border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-200 px-6 py-4">
          <h3 className="text-sm font-semibold text-slate-900">My correction requests</h3>
        </div>
        {myCorrections.length === 0 ? (
          <p className="px-6 py-8 text-center text-sm text-slate-500">No correction requests yet.</p>
        ) : (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="text-xs uppercase tracking-wide text-slate-500">
                <th className="px-6 py-2 font-medium">Original</th>
                <th className="px-6 py-2 font-medium">Requested</th>
                <th className="px-6 py-2 font-medium">Status</th>
                <th className="px-6 py-2 font-medium">Rejection reason</th>
              </tr>
            </thead>
            <tbody>
              {myCorrections.map((c) => (
                <tr key={c.id} className="border-t border-slate-100">
                  <td className="px-6 py-2 text-slate-700">
                    {formatDateTime(c.originalClockInUtc)} –{" "}
                    {c.originalClockOutUtc ? formatDateTime(c.originalClockOutUtc) : "in progress"}
                  </td>
                  <td className="px-6 py-2 text-slate-700">
                    {c.requestedClockInUtc ? formatDateTime(c.requestedClockInUtc) : "—"} –{" "}
                    {c.requestedClockOutUtc ? formatDateTime(c.requestedClockOutUtc) : "—"}
                  </td>
                  <td className="px-6 py-2">
                    <Badge
                      label={TimesheetCorrectionStatusLabels[c.status]}
                      tone={
                        c.status === TimesheetCorrectionStatus.Approved
                          ? "emerald"
                          : c.status === TimesheetCorrectionStatus.Rejected
                            ? "red"
                            : "amber"
                      }
                    />
                  </td>
                  <td className="px-6 py-2 text-slate-500">{c.rejectionReason ?? "—"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
