using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Domain.Entities;

public class KpiAppraisal
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public Guid KpiTemplateId { get; set; }
    public KpiTemplate? KpiTemplate { get; set; }

    public string PeriodLabel { get; set; } = string.Empty;
    public string? InductionNumber { get; set; }

    // Unconstrained scalar (no FK/navigation) to avoid a second cascade path to Employee —
    // mirrors LeaveRequest.LineManagerReviewerId/HRReviewerId, resolved manually in the service.
    public Guid CreatedByEmployeeId { get; set; }

    public DateOnly? Checkpoint1Date { get; set; }
    public DateOnly? Checkpoint2Date { get; set; }
    public DateOnly? Checkpoint3Date { get; set; }
    public DateOnly? Checkpoint4Date { get; set; }

    public Guid BlastingOfficerEmployeeId { get; set; }
    public DateTime? BlastingOfficerSignedAtUtc { get; set; }
    public byte[]? BlastingOfficerSignatureImageData { get; set; }

    public Guid BlastingEngineerEmployeeId { get; set; }
    public DateTime? BlastingEngineerSignedAtUtc { get; set; }
    public byte[]? BlastingEngineerSignatureImageData { get; set; }

    public KpiAppraisalStatus Status { get; set; } = KpiAppraisalStatus.InProgress;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastScoredAtUtc { get; set; }
    public DateTime? FinalizedAtUtc { get; set; }

    public List<KpiAppraisalItem> Items { get; set; } = [];
}
