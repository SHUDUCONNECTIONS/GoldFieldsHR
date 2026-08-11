namespace GoldFieldsHR.Domain.Entities;

public class Announcement
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public Guid PostedByEmployeeId { get; set; }
    public Employee? PostedByEmployee { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
