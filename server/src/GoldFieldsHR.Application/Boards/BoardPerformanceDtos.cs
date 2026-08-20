namespace GoldFieldsHR.Application.Boards;

/// <summary>Query-only filter, not persisted anywhere — how far back "completed" counts and the chart look.</summary>
public enum PerformanceRange
{
    Week = 0,
    Month = 1,
    All = 2,
}

public record PerformanceChartPointDto(string Label, DateOnly BucketStart, int TasksCompleted);

public record MyPerformanceDto(
    int TasksCompletedTotal,
    int TasksInProgress,
    int TasksOverdue,
    IReadOnlyList<PerformanceChartPointDto> Chart);

public record EmployeePerformanceDto(
    Guid EmployeeId,
    string EmployeeName,
    string SiteName,
    int TasksCompleted,
    int TasksInProgress,
    int TasksOverdue);
