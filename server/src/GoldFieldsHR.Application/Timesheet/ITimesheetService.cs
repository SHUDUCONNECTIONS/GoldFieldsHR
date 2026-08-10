using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Timesheet;

public interface ITimesheetService
{
    Task<Result<TimesheetEntryDto>> ClockInAsync(Guid employeeId, CancellationToken cancellationToken = default);

    Task<Result<TimesheetEntryDto>> ClockOutAsync(Guid employeeId, CancellationToken cancellationToken = default);

    Task<TimesheetEntryDto?> GetOpenEntryAsync(Guid employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimesheetEntryDto>> GetHistoryAsync(Guid employeeId, CancellationToken cancellationToken = default);

    Task<Result<TimesheetCorrectionDto>> SubmitCorrectionAsync(
        Guid employeeId, SubmitTimesheetCorrectionRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimesheetCorrectionDto>> GetMyCorrectionsAsync(
        Guid employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimesheetCorrectionDto>> GetPendingCorrectionApprovalsAsync(
        Guid reviewerId, CancellationToken cancellationToken = default);

    Task<Result<TimesheetCorrectionDto>> ReviewCorrectionAsync(
        Guid correctionId, Guid reviewerId, ReviewTimesheetCorrectionRequest review, CancellationToken cancellationToken = default);
}
