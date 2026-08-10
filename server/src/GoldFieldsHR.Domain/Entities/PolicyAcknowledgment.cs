namespace GoldFieldsHR.Domain.Entities;

public class PolicyAcknowledgment
{
    public Guid Id { get; set; }

    public Guid PolicyId { get; set; }
    public Policy? Policy { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime AcknowledgedAtUtc { get; set; } = DateTime.UtcNow;
}
