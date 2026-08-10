using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Domain.Entities;

public class TimesheetCorrectionRequest
{
    public Guid Id { get; set; }

    public Guid TimesheetEntryId { get; set; }
    public TimesheetEntry? TimesheetEntry { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime? RequestedClockInUtc { get; set; }
    public DateTime? RequestedClockOutUtc { get; set; }
    public string Reason { get; set; } = string.Empty;

    public TimesheetCorrectionStatus Status { get; set; } = TimesheetCorrectionStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Guid? ReviewerId { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public string? RejectionReason { get; set; }
}
