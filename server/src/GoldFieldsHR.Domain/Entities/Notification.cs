namespace GoldFieldsHR.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }

    public Guid RecipientEmployeeId { get; set; }
    public Employee? RecipientEmployee { get; set; }

    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
