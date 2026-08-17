namespace GoldFieldsHR.Domain.Entities;

public class KpiTemplateCategory
{
    public Guid Id { get; set; }

    public Guid KpiTemplateId { get; set; }
    public KpiTemplate? KpiTemplate { get; set; }

    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public List<KpiTemplateItem> Items { get; set; } = [];
}
