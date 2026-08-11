using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.Leave;

public record SubmitLeaveRequest(
    LeaveType LeaveType,
    DateOnly StartDate,
    DateOnly EndDate,
    string Reason,
    string ContactNumber);

public record ReviewLeaveRequest(bool Approve, string? RejectionReason);

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
    DateTime? ReviewedAtUtc,
    string? RejectionReason,
    bool IsDirectReport);
