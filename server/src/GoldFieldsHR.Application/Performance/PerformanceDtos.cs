namespace GoldFieldsHR.Application.Performance;

public record CreatePerformanceReviewRequest(
    string EmployeeNumber,
    string PeriodLabel,
    int Score,
    string? Comments);

public record PerformanceReviewDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    Guid ReviewerId,
    string ReviewerName,
    string PeriodLabel,
    int Score,
    string? Comments,
    DateTime CreatedAtUtc);
