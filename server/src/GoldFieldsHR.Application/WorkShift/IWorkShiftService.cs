using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.WorkShift;

public interface IWorkShiftService
{
    Task<Result<ShiftChangeRequestDto>> SubmitAsync(
        Guid employeeId, SubmitShiftChangeRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ShiftChangeRequestDto>> GetMyRequestsAsync(
        Guid employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ShiftChangeRequestDto>> GetPendingLineManagerApprovalsAsync(
        Guid reviewerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ShiftChangeRequestDto>> GetPendingHRApprovalsAsync(
        CancellationToken cancellationToken = default);

    Task<Result<ShiftChangeRequestDto>> LineManagerReviewAsync(
        Guid requestId, Guid reviewerId, ReviewShiftChangeRequest review, CancellationToken cancellationToken = default);

    Task<Result<ShiftChangeRequestDto>> HRReviewAsync(
        Guid requestId, Guid reviewerId, ReviewShiftChangeRequest review, CancellationToken cancellationToken = default);
}
