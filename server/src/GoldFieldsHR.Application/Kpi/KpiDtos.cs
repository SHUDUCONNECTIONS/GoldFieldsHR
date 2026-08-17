namespace GoldFieldsHR.Application.Kpi;

public record KpiTemplateItemDto(Guid Id, string Description, string? SubGroupLabel, int DisplayOrder);

public record KpiTemplateCategoryDto(Guid Id, string Name, int DisplayOrder, IReadOnlyList<KpiTemplateItemDto> Items);

public record KpiTemplateSummaryDto(
    Guid Id, string Designation, bool IsActive, int CategoryCount, int ItemCount, DateTime CreatedAtUtc);

public record KpiTemplateDetailDto(
    Guid Id, string Designation, bool IsActive, DateTime CreatedAtUtc, IReadOnlyList<KpiTemplateCategoryDto> Categories);

public record CreateKpiTemplateItemRequest(string Description, string? SubGroupLabel);

public record CreateKpiTemplateCategoryRequest(string Name, IReadOnlyList<CreateKpiTemplateItemRequest> Items);

public record CreateKpiTemplateRequest(string Designation, IReadOnlyList<CreateKpiTemplateCategoryRequest> Categories);

public record KpiAppraisalCategoryRollupDto(string Name, double? ScorePercent, int ItemCount);

public record KpiAppraisalSummaryDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeNumber,
    string Designation,
    string PeriodLabel,
    string Status,
    double? OverallScorePercent,
    IReadOnlyList<KpiAppraisalCategoryRollupDto> Categories,
    DateTime? LastReviewedAtUtc,
    IReadOnlyList<string> SignedOffBy,
    DateTime CreatedAtUtc);

public record KpiAppraisalItemDto(
    Guid Id,
    string CategoryName,
    string? SubGroupLabel,
    string Description,
    int DisplayOrder,
    bool? InPlace,
    bool? Ability,
    int? Checkpoint1Score,
    string? Checkpoint1Comment,
    int? Checkpoint2Score,
    string? Checkpoint2Comment,
    int? Checkpoint3Score,
    string? Checkpoint3Comment,
    int? Checkpoint4Score,
    string? Checkpoint4Comment,
    string? Evaluation);

public record KpiAppraisalDetailDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeNumber,
    string Designation,
    string PeriodLabel,
    string? InductionNumber,
    string Status,
    DateOnly? Checkpoint1Date,
    DateOnly? Checkpoint2Date,
    DateOnly? Checkpoint3Date,
    DateOnly? Checkpoint4Date,
    Guid BlastingOfficerEmployeeId,
    string BlastingOfficerName,
    DateTime? BlastingOfficerSignedAtUtc,
    Guid BlastingEngineerEmployeeId,
    string BlastingEngineerName,
    DateTime? BlastingEngineerSignedAtUtc,
    DateTime CreatedAtUtc,
    DateTime? FinalizedAtUtc,
    double? OverallScorePercent,
    IReadOnlyList<KpiAppraisalCategoryRollupDto> Categories,
    IReadOnlyList<KpiAppraisalItemDto> Items);

public record CreateKpiAppraisalRequest(
    string EmployeeNumber,
    Guid KpiTemplateId,
    string PeriodLabel,
    string? InductionNumber,
    string BlastingOfficerEmployeeNumber,
    string BlastingEngineerEmployeeNumber,
    DateOnly? Checkpoint1Date,
    DateOnly? Checkpoint2Date,
    DateOnly? Checkpoint3Date,
    DateOnly? Checkpoint4Date);

public record KpiItemScoreEntry(Guid ItemId, int Score, string? Comment);

public record SubmitCheckpointScoresRequest(int CheckpointNumber, IReadOnlyList<KpiItemScoreEntry> Items);

public record KpiItemFlagEntry(Guid ItemId, bool? InPlace, bool? Ability);

public record SetItemFlagsRequest(IReadOnlyList<KpiItemFlagEntry> Items);

public record SignKpiAppraisalRequest(string? SignaturePngBase64);
