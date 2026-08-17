namespace GoldFieldsHR.Application.Timesheet;

public record ClockingReportParseResultDto(
    string Filename,
    string Status,
    string Message,
    int? Events,
    int? Days,
    int? Shifts,
    double? TotalHours,
    string? XlsxBase64,
    string? DownloadName);
