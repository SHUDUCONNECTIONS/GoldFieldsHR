using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.Leave;

public record SubmitLeaveRequest(
    LeaveType LeaveType,
    DateOnly StartDate,
    DateOnly EndDate,
    string Reason,
    string ContactNumber);

public record ReviewLeaveRequest(bool Approve, string? RejectionReason, string? SignaturePngBase64);

public record LeaveRequestDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    LeaveType LeaveType,
    DateOnly StartDate,
    DateOnly EndDate,
    int DaysRequested,
    string Reason,
    string ContactNumber,
    LeaveRequestStatus Status,
    DateTime CreatedAtUtc,
    DateTime? LineManagerReviewedAtUtc,
    DateTime? HRReviewedAtUtc,
    string? RejectionReason,
    bool IsDirectReport);
