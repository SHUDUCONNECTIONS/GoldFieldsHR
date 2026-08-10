using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.Emergency;

public record TriggerEmergencyAlertRequest(string Location, string? Message);

public record ResolveEmergencyAlertRequest(string? ResolutionNotes);

public record EmergencyAlertDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    string Location,
    string? Message,
    EmergencyAlertStatus Status,
    DateTime TriggeredAtUtc,
    DateTime? ResolvedAtUtc,
    string? ResolutionNotes);
