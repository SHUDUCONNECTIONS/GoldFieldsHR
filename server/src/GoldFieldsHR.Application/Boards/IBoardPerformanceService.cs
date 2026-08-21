using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Boards;

public interface IBoardPerformanceService
{
    Task<MyPerformanceDto> GetMyPerformanceAsync(
        Guid employeeId, PerformanceRange range, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmployeePerformanceDto>> GetOrgPerformanceAsync(
        Guid? siteId, PerformanceRange range, CancellationToken cancellationToken = default);

    Task<OrgPerformanceSummaryDto> GetOrgSummaryAsync(Guid? siteId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CompletedBoardDto>> GetCompletedBoardsAsync(Guid? siteId, CancellationToken cancellationToken = default);

    Task<Result<byte[]>> GenerateEmployeePerformancePdfAsync(Guid employeeId, CancellationToken cancellationToken = default);
}
