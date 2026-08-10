using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.Timesheet;

public record TimesheetEntryDto(
    Guid Id,
    DateTime ClockInUtc,
    DateTime? ClockOutUtc,
    double? DurationHours);

public record SubmitTimesheetCorrectionRequest(
    Guid TimesheetEntryId,
    DateTime? RequestedClockInUtc,
    DateTime? RequestedClockOutUtc,
    string Reason);

public record ReviewTimesheetCorrectionRequest(bool Approve, string? RejectionReason);

public record TimesheetCorrectionDto(
    Guid Id,
    Guid TimesheetEntryId,
    Guid EmployeeId,
    string EmployeeName,
    DateTime OriginalClockInUtc,
    DateTime? OriginalClockOutUtc,
    DateTime? RequestedClockInUtc,
    DateTime? RequestedClockOutUtc,
    string Reason,
    TimesheetCorrectionStatus Status,
    DateTime CreatedAtUtc,
    DateTime? ReviewedAtUtc,
    string? RejectionReason,
    bool IsDirectReport);
