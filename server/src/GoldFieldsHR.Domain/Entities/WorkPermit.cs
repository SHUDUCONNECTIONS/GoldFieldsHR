using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Domain.Entities;

public class WorkPermit
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public PermitType PermitType { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateOnly ValidFrom { get; set; }
    public DateOnly ValidTo { get; set; }

    public PermitStatus Status { get; set; } = PermitStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Guid? ReviewerId { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public string? RejectionReason { get; set; }

    public DateTime? ClosedAtUtc { get; set; }
    public string? ClosedNotes { get; set; }
}
