using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Permits;

public interface IPermitService
{
    Task<Result<WorkPermitDto>> SubmitAsync(
        Guid employeeId, SubmitPermitRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkPermitDto>> GetMyPermitsAsync(
        Guid employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkPermitDto>> GetPendingApprovalsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkPermitDto>> GetOpenPermitsAsync(
        CancellationToken cancellationToken = default);

    Task<Result<WorkPermitDto>> ReviewAsync(
        Guid permitId, Guid reviewerId, ReviewPermitRequest review, CancellationToken cancellationToken = default);

    Task<Result<WorkPermitDto>> CloseAsync(
        Guid permitId, ClosePermitRequest request, CancellationToken cancellationToken = default);
}
