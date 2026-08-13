using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.LegalAppointments;

public record SubmitLegalAppointmentRequest(
    LegalAppointmentType AppointmentType,
    string AppointedBy,
    string Description,
    DateOnly ValidFrom,
    DateOnly ValidTo);

public record ReviewLegalAppointmentRequest(bool Approve, string? RejectionReason);

public record RevokeLegalAppointmentRequest(string? RevokedNotes);

public record LegalAppointmentDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    LegalAppointmentType AppointmentType,
    string AppointedBy,
    string Description,
    DateOnly ValidFrom,
    DateOnly ValidTo,
    LegalAppointmentStatus Status,
    DateTime CreatedAtUtc,
    DateTime? ReviewedAtUtc,
    string? RejectionReason,
    DateTime? RevokedAtUtc,
    string? RevokedNotes);
