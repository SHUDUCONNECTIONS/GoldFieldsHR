using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Domain.Entities;

public class EmergencyAlert
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public string Location { get; set; } = string.Empty;
    public string? Message { get; set; }

    public EmergencyAlertStatus Status { get; set; } = EmergencyAlertStatus.Active;
    public DateTime TriggeredAtUtc { get; set; } = DateTime.UtcNow;

    public Guid? ResolvedByEmployeeId { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public string? ResolutionNotes { get; set; }
}
