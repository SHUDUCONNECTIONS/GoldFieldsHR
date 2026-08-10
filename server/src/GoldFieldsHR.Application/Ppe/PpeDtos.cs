using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.Ppe;

public record SubmitPpeRequest(
    PpeItemType ItemType,
    string? Size,
    int Quantity,
    string Reason);

public record ReviewPpeRequest(bool Approve, string? RejectionReason);

public record PpeRequestDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    PpeItemType ItemType,
    string? Size,
    int Quantity,
    string Reason,
    PpeRequestStatus Status,
    DateTime CreatedAtUtc,
    DateTime? ReviewedAtUtc,
    string? RejectionReason,
    DateTime? IssuedAtUtc);
