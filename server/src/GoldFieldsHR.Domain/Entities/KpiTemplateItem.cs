namespace GoldFieldsHR.Domain.Entities;

public class KpiTemplateItem
{
    public Guid Id { get; set; }

    public Guid KpiTemplateCategoryId { get; set; }
    public KpiTemplateCategory? KpiTemplateCategory { get; set; }

    public string Description { get; set; } = string.Empty;

    // Plain display grouping label seen under some categories (e.g. "Daily"/"Weekly"/"Monthly"
    // under QUALITY & QUANTITY BLAST) — not a real nested category, just rendered above the item.
    public string? SubGroupLabel { get; set; }

    public int DisplayOrder { get; set; }
}
