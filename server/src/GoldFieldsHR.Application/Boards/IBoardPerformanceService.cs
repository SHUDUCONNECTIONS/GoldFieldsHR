namespace GoldFieldsHR.Application.Boards;

public interface IBoardPerformanceService
{
    Task<MyPerformanceDto> GetMyPerformanceAsync(
        Guid employeeId, PerformanceRange range, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmployeePerformanceDto>> GetOrgPerformanceAsync(
        Guid? siteId, PerformanceRange range, CancellationToken cancellationToken = default);
}
