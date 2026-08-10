namespace GoldFieldsHR.Domain.Entities;

public class PerformanceReview
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public Guid ReviewerId { get; set; }
    public Employee? Reviewer { get; set; }

    public string PeriodLabel { get; set; } = string.Empty;
    public int Score { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
