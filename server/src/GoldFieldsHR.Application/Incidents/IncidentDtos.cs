using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.Incidents;

public record SubmitIncidentReportRequest(
    string Title,
    string Description,
    IncidentSeverity Severity,
    string Location,
    DateTime OccurredAtUtc);

public record UpdateIncidentStatusRequest(IncidentStatus Status, string? ReviewNotes);

public record IncidentReportDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    string Title,
    string Description,
    IncidentSeverity Severity,
    string Location,
    DateTime OccurredAtUtc,
    IncidentStatus Status,
    DateTime CreatedAtUtc,
    DateTime? ReviewedAtUtc,
    string? ReviewNotes);
