namespace GoldFieldsHR.Domain.Entities;

public class Certificate
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public string Title { get; set; } = string.Empty;
    public DateOnly IssuedDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public string? Notes { get; set; }

    public Guid IssuedByEmployeeId { get; set; }
    public Employee? IssuedByEmployee { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
