using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Leave;

public interface ILeaveService
{
    Task<Result<LeaveRequestDto>> SubmitAsync(
        Guid employeeId, SubmitLeaveRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaveRequestDto>> GetMyRequestsAsync(
        Guid employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaveRequestDto>> GetPendingLineManagerApprovalsAsync(
        Guid reviewerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaveRequestDto>> GetPendingHRApprovalsAsync(
        CancellationToken cancellationToken = default);

    Task<Result<LeaveRequestDto>> LineManagerReviewAsync(
        Guid requestId, Guid reviewerId, ReviewLeaveRequest review, CancellationToken cancellationToken = default);

    Task<Result<LeaveRequestDto>> HRReviewAsync(
        Guid requestId, Guid reviewerId, ReviewLeaveRequest review, CancellationToken cancellationToken = default);

    Task<Result<byte[]>> GenerateSignedDocumentAsync(
        Guid requestId, Guid requesterId, CancellationToken cancellationToken = default);
}
