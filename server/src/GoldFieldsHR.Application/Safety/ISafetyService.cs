using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Safety;

public interface ISafetyService
{
    Task<Result<PreShiftSafetyCheckDto>> SubmitAsync(
        Guid employeeId, SubmitPreShiftCheckRequest request, CancellationToken cancellationToken = default);

    Task<PreShiftSafetyCheckDto?> GetTodayAsync(Guid employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PreShiftSafetyCheckDto>> GetHistoryAsync(
        Guid employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PreShiftSafetyCheckDto>> GetTodaysHazardsAsync(CancellationToken cancellationToken = default);
}
