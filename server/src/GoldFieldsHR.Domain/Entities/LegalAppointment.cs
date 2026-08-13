using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Domain.Entities;

public class LegalAppointment
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public LegalAppointmentType AppointmentType { get; set; }
    public string AppointedBy { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateOnly ValidFrom { get; set; }
    public DateOnly ValidTo { get; set; }

    public LegalAppointmentStatus Status { get; set; } = LegalAppointmentStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Guid? ReviewerId { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public string? RejectionReason { get; set; }

    public DateTime? RevokedAtUtc { get; set; }
    public string? RevokedNotes { get; set; }
}
