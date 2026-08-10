using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.Permits;

public record SubmitPermitRequest(
    PermitType PermitType,
    string Location,
    string Description,
    DateOnly ValidFrom,
    DateOnly ValidTo);

public record ReviewPermitRequest(bool Approve, string? RejectionReason);

public record ClosePermitRequest(string? ClosedNotes);

public record WorkPermitDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    PermitType PermitType,
    string Location,
    string Description,
    DateOnly ValidFrom,
    DateOnly ValidTo,
    PermitStatus Status,
    DateTime CreatedAtUtc,
    DateTime? ReviewedAtUtc,
    string? RejectionReason,
    DateTime? ClosedAtUtc,
    string? ClosedNotes);
