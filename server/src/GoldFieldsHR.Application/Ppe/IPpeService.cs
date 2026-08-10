using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Ppe;

public interface IPpeService
{
    Task<Result<PpeRequestDto>> SubmitAsync(
        Guid employeeId, SubmitPpeRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PpeRequestDto>> GetMyRequestsAsync(
        Guid employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PpeRequestDto>> GetPendingApprovalsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PpeRequestDto>> GetAwaitingIssueAsync(
        CancellationToken cancellationToken = default);

    Task<Result<PpeRequestDto>> ReviewAsync(
        Guid requestId, Guid reviewerId, ReviewPpeRequest review, CancellationToken cancellationToken = default);

    Task<Result<PpeRequestDto>> MarkIssuedAsync(
        Guid requestId, Guid issuerId, CancellationToken cancellationToken = default);
}
