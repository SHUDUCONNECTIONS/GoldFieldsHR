namespace GoldFieldsHR.Domain.Entities;

public class BoardMember
{
    public Guid Id { get; set; }

    public Guid BoardId { get; set; }
    public Board? Board { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;
}
