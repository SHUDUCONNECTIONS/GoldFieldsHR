using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.WorkShift;

public record SubmitShiftChangeRequest(
    DateOnly RequestedShiftDate,
    ShiftType RequestedShiftType,
    string Reason,
    string? Comments);

public record ReviewShiftChangeRequest(bool Approve, string? RejectionReason);

public record ShiftChangeRequestDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    DateOnly RequestedShiftDate,
    ShiftType RequestedShiftType,
    string Reason,
    string? Comments,
    ShiftRequestStatus Status,
    DateTime CreatedAtUtc,
    DateTime? LineManagerReviewedAtUtc,
    DateTime? HRReviewedAtUtc,
    string? RejectionReason,
    bool IsDirectReport);
