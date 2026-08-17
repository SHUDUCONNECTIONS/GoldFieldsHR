namespace GoldFieldsHR.Domain.Entities;

public class KpiTemplate
{
    public Guid Id { get; set; }

    public string Designation { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<KpiTemplateCategory> Categories { get; set; } = [];
}
