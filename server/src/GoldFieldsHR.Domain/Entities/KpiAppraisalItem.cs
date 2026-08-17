namespace GoldFieldsHR.Domain.Entities;

public class KpiAppraisalItem
{
    public Guid Id { get; set; }

    public Guid KpiAppraisalId { get; set; }
    public KpiAppraisal? KpiAppraisal { get; set; }

    public Guid KpiTemplateItemId { get; set; }
    public KpiTemplateItem? KpiTemplateItem { get; set; }

    // Snapshotted at appraisal-creation time so later template edits never retroactively
    // rewrite the text on an appraisal that's already in progress or finalized.
    public string DescriptionSnapshot { get; set; } = string.Empty;
    public string CategoryNameSnapshot { get; set; } = string.Empty;
    public string? SubGroupLabelSnapshot { get; set; }
    public int DisplayOrder { get; set; }

    public bool? InPlace { get; set; }
    public bool? Ability { get; set; }

    public int? Checkpoint1Score { get; set; }
    public string? Checkpoint1Comment { get; set; }
    public int? Checkpoint2Score { get; set; }
    public string? Checkpoint2Comment { get; set; }
    public int? Checkpoint3Score { get; set; }
    public string? Checkpoint3Comment { get; set; }
    public int? Checkpoint4Score { get; set; }
    public string? Checkpoint4Comment { get; set; }

    public string? Evaluation { get; set; }
}
