import { apiClient } from "./client";
import type { ClockingReportParseResultDto } from "../types/clockingParser";

export interface ParseSchedule {
  workDays: string[];
  hoursPerDay: number;
  rotating: boolean;
}

export async function parseClockingReport(file: File, schedule: ParseSchedule): Promise<ClockingReportParseResultDto> {
  const form = new FormData();
  form.append("file", file);
  form.append("workDays", schedule.workDays.join(","));
  form.append("hoursPerDay", String(schedule.hoursPerDay));
  form.append("rotating", String(schedule.rotating));

  const { data } = await apiClient.post<ClockingReportParseResultDto>("/timesheet/clocking-report-parser", form, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return data;
}

export function downloadClockingParseResult(result: ClockingReportParseResultDto): void {
  if (!result.xlsxBase64) return;
  const bytes = Uint8Array.from(atob(result.xlsxBase64), (c) => c.charCodeAt(0));
  const blob = new Blob([bytes], {
    type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
  });
  const url = URL.createObjectURL(blob);

  const a = document.createElement("a");
  a.href = url;
  a.download = result.downloadName ?? result.filename.replace(/\.pdf$/i, "_parsed.xlsx");
  document.body.appendChild(a);
  a.click();
  a.remove();
  URL.revokeObjectURL(url);
}
