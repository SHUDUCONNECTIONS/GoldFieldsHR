using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Domain.Entities;

public class LeaveRequest
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public LeaveType LeaveType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;

    public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Guid? ReviewerId { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public string? RejectionReason { get; set; }
}
