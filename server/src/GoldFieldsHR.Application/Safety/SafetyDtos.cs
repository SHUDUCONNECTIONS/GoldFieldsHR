namespace GoldFieldsHR.Application.Safety;

public record SubmitPreShiftCheckRequest(bool HazardsIdentified, string? HazardNotes);

public record PreShiftSafetyCheckDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    DateOnly CheckDate,
    bool HazardsIdentified,
    string? HazardNotes,
    DateTime SubmittedAtUtc);
