export interface ClockingReportParseResultDto {
  filename: string;
  status: "ok" | "error";
  message: string;
  events: number | null;
  days: number | null;
  shifts: number | null;
  totalHours: number | null;
  xlsxBase64: string | null;
  downloadName: string | null;
}

export type ClockingParseJobStatus = "queued" | "parsing" | "ok" | "error";

export interface ClockingParseJob {
  clientId: string;
  file: File;
  status: ClockingParseJobStatus;
  result?: ClockingReportParseResultDto;
}
