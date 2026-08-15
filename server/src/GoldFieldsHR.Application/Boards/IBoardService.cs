using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Boards;

public interface IBoardService
{
    Task<Result<BoardDto>> CreateAsync(
        Guid ownerEmployeeId, CreateBoardRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BoardDto>> GetMineAsync(
        Guid employeeId, CancellationToken cancellationToken = default);

    Task<Result<BoardDto>> GetByIdAsync(
        Guid boardId, Guid requesterId, CancellationToken cancellationToken = default);

    Task<Result<BoardDto>> UpdateAsync(
        Guid boardId, Guid ownerEmployeeId, UpdateBoardRequest request, CancellationToken cancellationToken = default);

    Task<Result<BoardDto>> AddMemberAsync(
        Guid boardId, Guid ownerEmployeeId, AddBoardMemberRequest request, CancellationToken cancellationToken = default);

    Task<Result<BoardDto>> RemoveMemberAsync(
        Guid boardId, Guid ownerEmployeeId, Guid employeeId, CancellationToken cancellationToken = default);
}
