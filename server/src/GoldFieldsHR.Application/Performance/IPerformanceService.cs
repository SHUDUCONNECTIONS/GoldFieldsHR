using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Performance;

public interface IPerformanceService
{
    Task<Result<PerformanceReviewDto>> CreateAsync(
        Guid reviewerEmployeeId, CreatePerformanceReviewRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PerformanceReviewDto>> GetMyReviewsAsync(
        Guid employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PerformanceReviewDto>> GetGivenByMeAsync(
        Guid reviewerEmployeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PerformanceReviewDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
