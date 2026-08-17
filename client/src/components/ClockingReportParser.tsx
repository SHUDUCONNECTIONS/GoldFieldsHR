import { useCallback, useRef, useState } from "react";
import { Download, Upload, X } from "lucide-react";
import { downloadClockingParseResult, parseClockingReport } from "../api/clockingParser";
import type { ClockingParseJob, ClockingReportParseResultDto } from "../types/clockingParser";

// The working week is Mon-Fri, optionally extended to include Saturday for
// employees scheduled to work it (Sunday is never part of this — Sunday
// work always goes to S/T regardless of schedule, see the parser).
const BASE_WORK_DAYS = ["mon", "tue", "wed", "thu", "fri"];
const SHIFT_OPTIONS = [10, 9, 8];

function makeId(): string {
  return Math.random().toString(36).slice(2) + Date.now().toString(36);
}

function StatusBadge({ status }: { status: ClockingParseJob["status"] }) {
  switch (status) {
    case "queued":
      return <span className="rounded-full bg-slate-100 px-2 py-0.5 text-[10px] font-semibold text-slate-600">Queued</span>;
    case "parsing":
      return (
        <span className="inline-flex items-center gap-1 rounded-full bg-yellow-100 px-2 py-0.5 text-[10px] font-semibold text-yellow-700">
          <span className="h-2 w-2 animate-spin rounded-full border-2 border-yellow-500 border-t-transparent" />
          Parsing
        </span>
      );
    case "ok":
      return <span className="rounded-full bg-emerald-100 px-2 py-0.5 text-[10px] font-semibold text-emerald-700">Parsed</span>;
    case "error":
      return <span className="rounded-full bg-red-100 px-2 py-0.5 text-[10px] font-semibold text-red-700">Failed</span>;
  }
}

export function ClockingReportParser() {
  const [jobs, setJobs] = useState<ClockingParseJob[]>([]);
  const [isDragging, setIsDragging] = useState(false);
  const [isRunning, setIsRunning] = useState(false);
  const [hoursPerDay, setHoursPerDay] = useState<number | null>(null);
  const [includeSaturday, setIncludeSaturday] = useState(false);
  // The 10h shift is a rotating 4-on/4-off pattern, not a fixed weekly one.
  const rotating = hoursPerDay === 10;
  const fileInputRef = useRef<HTMLInputElement>(null);
  const dragCounter = useRef(0);

  const addFiles = useCallback((fileList: FileList | File[]) => {
    const pdfs = Array.from(fileList).filter((f) => f.name.toLowerCase().endsWith(".pdf"));
    if (pdfs.length === 0) return;
    setJobs((prev) => [...prev, ...pdfs.map((file) => ({ clientId: makeId(), file, status: "queued" as const }))]);
  }, []);

  function onDrop(event: React.DragEvent) {
    event.preventDefault();
    dragCounter.current = 0;
    setIsDragging(false);
    if (event.dataTransfer.files?.length) addFiles(event.dataTransfer.files);
  }

  function onDragEnter(event: React.DragEvent) {
    event.preventDefault();
    dragCounter.current += 1;
    setIsDragging(true);
  }

  function onDragLeave(event: React.DragEvent) {
    event.preventDefault();
    dragCounter.current -= 1;
    if (dragCounter.current <= 0) setIsDragging(false);
  }

  function removeJob(id: string) {
    setJobs((prev) => prev.filter((j) => j.clientId !== id));
  }

  function clearAll() {
    setJobs([]);
  }

  function errorResult(filename: string, message: string): ClockingReportParseResultDto {
    return {
      filename,
      status: "error",
      message,
      events: null,
      days: null,
      shifts: null,
      totalHours: null,
      xlsxBase64: null,
      downloadName: null,
    };
  }

  async function runQueue() {
    if (hoursPerDay === null) return;
    setIsRunning(true);
    const toRun = jobs.filter((j) => j.status === "queued").map((j) => j.clientId);

    for (const id of toRun) {
      setJobs((prev) => prev.map((j) => (j.clientId === id ? { ...j, status: "parsing" } : j)));
      const job = jobs.find((j) => j.clientId === id);
      if (!job) continue;

      try {
        const result = await parseClockingReport(job.file, {
          workDays: includeSaturday ? [...BASE_WORK_DAYS, "sat"] : BASE_WORK_DAYS,
          hoursPerDay,
          rotating,
        });
        setJobs((prev) => prev.map((j) => (j.clientId === id ? { ...j, status: result.status, result } : j)));
      } catch (err) {
        const message = err instanceof Error ? err.message : "Upload failed";
        setJobs((prev) =>
          prev.map((j) => (j.clientId === id ? { ...j, status: "error", result: errorResult(job.file.name, message) } : j)),
        );
      }
    }
    setIsRunning(false);
  }

  function downloadAll() {
    for (const j of jobs) {
      if (j.status === "ok" && j.result) downloadClockingParseResult(j.result);
    }
  }

  const queuedCount = jobs.filter((j) => j.status === "queued").length;
  const okCount = jobs.filter((j) => j.status === "ok").length;
  const errorCount = jobs.filter((j) => j.status === "error").length;

  return (
    <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
      <h3 className="mb-1 text-sm font-semibold text-slate-900">Clocking report parser</h3>
      <p className="mb-4 text-xs text-slate-500">
        Upload an Individual Clocking History PDF from the turnstile system — get back a formatted Timesheet
        workbook. Nothing is stored; each PDF is parsed and discarded once you download the result.
      </p>

      {hoursPerDay === null ? (
        <div>
          <p className="mb-3 text-sm font-medium text-slate-700">What's the planned shift length?</p>
          <div className="grid grid-cols-3 gap-3">
            {SHIFT_OPTIONS.map((h) => (
              <button
                key={h}
                type="button"
                onClick={() => setHoursPerDay(h)}
                className="flex flex-col items-center gap-1 rounded-lg border border-slate-200 bg-slate-50 py-6 hover:border-yellow-500 hover:bg-yellow-50"
              >
                <span className="text-2xl font-bold text-slate-900">{h}</span>
                <span className="text-xs text-slate-500">hour shift</span>
              </button>
            ))}
          </div>
          <p className="mt-3 text-xs text-slate-500">
            Time worked beyond this goes to overtime — Sunday work always does. The 10h shift is treated as a
            rotating 4-on/4-off pattern; 9h and 8h follow a Mon–Fri (or Mon–Sat) week. You can change this later.
          </p>
        </div>
      ) : (
        <div>
          <div
            onDrop={onDrop}
            onDragEnter={onDragEnter}
            onDragLeave={onDragLeave}
            onDragOver={(e) => e.preventDefault()}
            onClick={() => fileInputRef.current?.click()}
            role="button"
            tabIndex={0}
            className={`flex cursor-pointer flex-col items-center gap-2 rounded-lg border-2 border-dashed px-6 py-8 text-center transition-colors ${
              isDragging ? "border-yellow-500 bg-yellow-50" : "border-slate-300 hover:border-slate-400"
            }`}
          >
            <input
              ref={fileInputRef}
              type="file"
              accept="application/pdf"
              multiple
              hidden
              onChange={(e) => e.target.files && addFiles(e.target.files)}
            />
            <Upload className="h-6 w-6 text-slate-400" />
            <p className="text-sm font-medium text-slate-700">Drag &amp; drop PDFs here</p>
            <p className="text-xs text-slate-500">Multiple files at once are fine</p>
            <span className="mt-1 rounded-full bg-slate-100 px-3 py-1 text-xs font-medium text-slate-600">Browse files</span>
          </div>

          <div className="mt-4 rounded-md bg-slate-50 p-3">
            <p className="text-xs font-medium text-slate-700">
              Schedule used to guess Shift / Planned hours
              <span className="font-normal text-slate-500"> — there's no roster in the PDF, this is a guess</span>
            </p>
            {rotating ? (
              <p className="mt-1 text-xs text-slate-500">
                Rotating shift (e.g. 4 days on, 4 off) — any day with a clocking counts as a scheduled {hoursPerDay}h
                day, no fixed week.
              </p>
            ) : (
              <div className="mt-2 inline-flex rounded-md border border-slate-300 p-0.5">
                <button
                  type="button"
                  onClick={() => setIncludeSaturday(false)}
                  className={`rounded px-3 py-1 text-xs font-medium ${!includeSaturday ? "bg-yellow-600 text-white" : "text-slate-600"}`}
                >
                  Mon–Fri
                </button>
                <button
                  type="button"
                  onClick={() => setIncludeSaturday(true)}
                  className={`rounded px-3 py-1 text-xs font-medium ${includeSaturday ? "bg-yellow-600 text-white" : "text-slate-600"}`}
                >
                  Mon–Sat
                </button>
              </div>
            )}
            <p className="mt-2 text-xs text-slate-600">
              Planned shift <strong>{hoursPerDay}h</strong>/day
              <button type="button" onClick={() => setHoursPerDay(null)} className="ml-2 text-yellow-700 hover:underline">
                Change
              </button>
            </p>
          </div>

          {jobs.length > 0 && (
            <>
              <div className="mt-4 flex flex-wrap items-center justify-between gap-2">
                <div className="flex items-center gap-2 text-xs text-slate-500">
                  {jobs.length} file{jobs.length !== 1 && "s"}
                  {okCount > 0 && <span className="text-emerald-600">{okCount} parsed</span>}
                  {errorCount > 0 && <span className="text-red-600">{errorCount} failed</span>}
                </div>
                <div className="flex gap-2">
                  {okCount > 1 && (
                    <button type="button" onClick={downloadAll} className="text-xs font-medium text-yellow-700 hover:underline">
                      Download all
                    </button>
                  )}
                  <button
                    type="button"
                    onClick={clearAll}
                    disabled={isRunning}
                    className="rounded-md border border-slate-300 px-2.5 py-1 text-xs font-medium text-slate-700 hover:bg-slate-50 disabled:opacity-50"
                  >
                    Clear
                  </button>
                  <button
                    type="button"
                    onClick={runQueue}
                    disabled={isRunning || queuedCount === 0}
                    className="rounded-md bg-yellow-600 px-3 py-1 text-xs font-medium text-white hover:bg-yellow-500 disabled:opacity-50"
                  >
                    {isRunning ? "Parsing..." : queuedCount > 0 ? `Parse ${queuedCount} file${queuedCount !== 1 ? "s" : ""}` : "All done"}
                  </button>
                </div>
              </div>

              <ul className="mt-3 divide-y divide-slate-100 rounded-md border border-slate-200">
                {jobs.map((job) => (
                  <li key={job.clientId} className="flex items-center justify-between gap-3 px-3 py-2">
                    <div className="min-w-0">
                      <p className="truncate text-sm text-slate-800">{job.file.name}</p>
                      {job.status === "ok" && job.result && (
                        <p className="text-xs text-slate-500">
                          {job.result.message} · {job.result.days}d · {job.result.totalHours}h total
                        </p>
                      )}
                      {job.status === "error" && job.result && (
                        <p className="text-xs text-red-600">{job.result.message}</p>
                      )}
                    </div>
                    <div className="flex shrink-0 items-center gap-2">
                      <StatusBadge status={job.status} />
                      {job.status === "ok" && job.result && (
                        <button
                          type="button"
                          onClick={() => downloadClockingParseResult(job.result!)}
                          aria-label="Download workbook"
                          className="text-slate-500 hover:text-yellow-700"
                        >
                          <Download className="h-4 w-4" />
                        </button>
                      )}
                      {job.status !== "parsing" && (
                        <button
                          type="button"
                          onClick={() => removeJob(job.clientId)}
                          aria-label="Remove"
                          className="text-slate-400 hover:text-slate-600"
                        >
                          <X className="h-4 w-4" />
                        </button>
                      )}
                    </div>
                  </li>
                ))}
              </ul>

              {okCount > 0 && (
                <p className="mt-2 text-xs text-slate-500">
                  First time opening a downloaded workbook? Excel may open it in "Protected View" since it came from
                  the browser — click <strong>Enable Editing</strong> in the yellow banner at the top to edit it.
                </p>
              )}
            </>
          )}
        </div>
      )}
    </div>
  );
}
