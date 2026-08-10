using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Domain.Entities;

public class PpeRequest
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public PpeItemType ItemType { get; set; }
    public string? Size { get; set; }
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;

    public PpeRequestStatus Status { get; set; } = PpeRequestStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Guid? ReviewerId { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? IssuedAtUtc { get; set; }
}
