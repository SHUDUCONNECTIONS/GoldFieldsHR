namespace GoldFieldsHR.Domain.Entities;

public class Policy
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public Guid PublishedByEmployeeId { get; set; }
    public Employee? PublishedByEmployee { get; set; }
    public DateTime PublishedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<PolicyAcknowledgment> Acknowledgments { get; set; } = new List<PolicyAcknowledgment>();
}
