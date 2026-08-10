namespace GoldFieldsHR.Domain.Entities;

public class PreShiftSafetyCheck
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateOnly CheckDate { get; set; }
    public bool HazardsIdentified { get; set; }
    public string? HazardNotes { get; set; }
    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
}
