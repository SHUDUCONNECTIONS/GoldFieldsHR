namespace GoldFieldsHR.Domain.Entities;

public class PostedScheduleDocument
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public Guid PostedByEmployeeId { get; set; }
    public Employee? PostedByEmployee { get; set; }
    public DateTime PostedAtUtc { get; set; } = DateTime.UtcNow;
}
