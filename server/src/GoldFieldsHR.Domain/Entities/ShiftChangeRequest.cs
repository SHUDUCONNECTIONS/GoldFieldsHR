using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Domain.Entities;

public class ShiftChangeRequest
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateOnly RequestedShiftDate { get; set; }
    public ShiftType RequestedShiftType { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Comments { get; set; }

    public ShiftRequestStatus Status { get; set; } = ShiftRequestStatus.PendingLineManagerApproval;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Guid? LineManagerReviewerId { get; set; }
    public DateTime? LineManagerReviewedAtUtc { get; set; }

    public Guid? HRReviewerId { get; set; }
    public DateTime? HRReviewedAtUtc { get; set; }

    public string? RejectionReason { get; set; }
}
