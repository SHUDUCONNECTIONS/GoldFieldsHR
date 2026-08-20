using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Boards;

public interface IBoardTaskService
{
    Task<Result<BoardTaskDto>> CreateAsync(
        Guid boardId, Guid requesterId, CreateBoardTaskRequest request, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<BoardTaskDto>>> GetForBoardAsync(
        Guid boardId, Guid requesterId, Domain.Enums.BoardTaskStatus? status, Guid? assigneeId,
        CancellationToken cancellationToken = default);

    Task<Result<BoardTaskDto>> GetByIdAsync(
        Guid boardId, Guid taskId, Guid requesterId, CancellationToken cancellationToken = default);

    Task<Result<BoardTaskDto>> UpdateAsync(
        Guid boardId, Guid taskId, Guid requesterId, UpdateBoardTaskRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<BoardTaskDto>> ChangeStatusAsync(
        Guid boardId, Guid taskId, Guid requesterId, ChangeTaskStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> DeleteAsync(
        Guid boardId, Guid taskId, Guid ownerEmployeeId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<WeeklyTaskCompletionDto>>> GetWeeklySummaryAsync(
        Guid boardId, Guid ownerEmployeeId, DateOnly? weekStartUtc, CancellationToken cancellationToken = default);

    Task<Result<byte[]>> GenerateWeeklySummaryPdfAsync(
        Guid boardId, Guid ownerEmployeeId, DateOnly? weekStartUtc, CancellationToken cancellationToken = default);
}
