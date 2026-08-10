using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Leave;

public interface ILeaveService
{
    Task<Result<LeaveRequestDto>> SubmitAsync(
        Guid employeeId, SubmitLeaveRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaveRequestDto>> GetMyRequestsAsync(
        Guid employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaveRequestDto>> GetPendingApprovalsAsync(
        Guid reviewerId, CancellationToken cancellationToken = default);

    Task<Result<LeaveRequestDto>> ReviewAsync(
        Guid requestId, Guid reviewerId, ReviewLeaveRequest review, CancellationToken cancellationToken = default);
}
