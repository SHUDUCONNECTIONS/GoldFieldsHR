using GoldFieldsHR.Application.Boards;
using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Notifications;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Boards;

public class BoardService(ApplicationDbContext dbContext, INotificationService notificationService) : IBoardService
{
    public async Task<Result<BoardDto>> CreateAsync(
        Guid ownerEmployeeId, CreateBoardRequest request, CancellationToken cancellationToken = default)
    {
        var owner = await dbContext.Employees.FindAsync([ownerEmployeeId], cancellationToken);
        if (owner is null)
        {
            return Result<BoardDto>.Failure("Employee profile not found.");
        }

        if (request.SiteId.HasValue)
        {
            var siteExists = await dbContext.Sites.AnyAsync(s => s.Id == request.SiteId.Value, cancellationToken);
            if (!siteExists)
            {
                return Result<BoardDto>.Failure("The selected site could not be found.");
            }
        }

        var memberIds = request.InitialMemberEmployeeIds.Distinct().Where(id => id != ownerEmployeeId).ToList();
        if (memberIds.Count > 0)
        {
            var validCount = await dbContext.Employees.CountAsync(e => memberIds.Contains(e.Id), cancellationToken);
            if (validCount != memberIds.Count)
            {
                return Result<BoardDto>.Failure("One or more selected members could not be found.");
            }
        }

        var board = new Board
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            OwnerEmployeeId = ownerEmployeeId,
            SiteId = request.SiteId,
            Priority = request.Priority,
            Deadline = request.Deadline,
        };

        board.Members.Add(new BoardMember { Id = Guid.NewGuid(), BoardId = board.Id, EmployeeId = ownerEmployeeId });
        foreach (var memberId in memberIds)
        {
            board.Members.Add(new BoardMember { Id = Guid.NewGuid(), BoardId = board.Id, EmployeeId = memberId });
        }

        dbContext.Boards.Add(board);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (memberIds.Count > 0)
        {
            await notificationService.CreateForManyAsync(
                memberIds, $"You were added to the board \"{board.Name}\".", $"/kpi/boards/{board.Id}", cancellationToken);
        }

        return Result<BoardDto>.Success(await LoadDtoAsync(board.Id, cancellationToken));
    }

    public async Task<IReadOnlyList<BoardDto>> GetMineAsync(
        Guid employeeId, CancellationToken cancellationToken = default)
    {
        var boardIds = await dbContext.Boards
            .Where(b => b.OwnerEmployeeId == employeeId || b.Members.Any(m => m.EmployeeId == employeeId))
            .OrderByDescending(b => b.CreatedAtUtc)
            .Select(b => b.Id)
            .ToListAsync(cancellationToken);

        var boards = new List<BoardDto>();
        foreach (var boardId in boardIds)
        {
            boards.Add(await LoadDtoAsync(boardId, cancellationToken));
        }

        return boards;
    }

    public async Task<Result<BoardDto>> GetByIdAsync(
        Guid boardId, Guid requesterId, CancellationToken cancellationToken = default)
    {
        var access = await CheckAccessAsync(boardId, requesterId, cancellationToken);
        if (access is null)
        {
            return Result<BoardDto>.Failure("Board not found.");
        }

        if (!access.Value.IsOwner && !access.Value.IsMember)
        {
            return Result<BoardDto>.Failure("You are not a member of this board.");
        }

        return Result<BoardDto>.Success(await LoadDtoAsync(boardId, cancellationToken));
    }

    public async Task<Result<BoardDto>> UpdateAsync(
        Guid boardId, Guid ownerEmployeeId, UpdateBoardRequest request, CancellationToken cancellationToken = default)
    {
        var board = await dbContext.Boards.FirstOrDefaultAsync(b => b.Id == boardId, cancellationToken);
        if (board is null)
        {
            return Result<BoardDto>.Failure("Board not found.");
        }

        if (board.OwnerEmployeeId != ownerEmployeeId)
        {
            return Result<BoardDto>.Failure("Only the board owner can update this board.");
        }

        board.Name = request.Name;
        board.Description = request.Description;
        board.IsArchived = request.IsArchived;
        board.Priority = request.Priority;
        board.Status = request.Status;
        board.Deadline = request.Deadline;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<BoardDto>.Success(await LoadDtoAsync(boardId, cancellationToken));
    }

    public async Task<Result<BoardDto>> AddMemberAsync(
        Guid boardId, Guid ownerEmployeeId, AddBoardMemberRequest request, CancellationToken cancellationToken = default)
    {
        var board = await dbContext.Boards.FirstOrDefaultAsync(b => b.Id == boardId, cancellationToken);
        if (board is null)
        {
            return Result<BoardDto>.Failure("Board not found.");
        }

        if (board.OwnerEmployeeId != ownerEmployeeId)
        {
            return Result<BoardDto>.Failure("Only the board owner can add members.");
        }

        var employeeExists = await dbContext.Employees.AnyAsync(e => e.Id == request.EmployeeId, cancellationToken);
        if (!employeeExists)
        {
            return Result<BoardDto>.Failure("Employee not found.");
        }

        var alreadyMember = await dbContext.BoardMembers
            .AnyAsync(m => m.BoardId == boardId && m.EmployeeId == request.EmployeeId, cancellationToken);
        if (alreadyMember)
        {
            return Result<BoardDto>.Failure("This employee is already a member of the board.");
        }

        dbContext.BoardMembers.Add(new BoardMember { Id = Guid.NewGuid(), BoardId = boardId, EmployeeId = request.EmployeeId });
        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.CreateAsync(
            request.EmployeeId, $"You were added to the board \"{board.Name}\".", $"/kpi/boards/{board.Id}", cancellationToken);

        return Result<BoardDto>.Success(await LoadDtoAsync(boardId, cancellationToken));
    }

    public async Task<Result<BoardDto>> RemoveMemberAsync(
        Guid boardId, Guid ownerEmployeeId, Guid employeeId, CancellationToken cancellationToken = default)
    {
        var board = await dbContext.Boards.FirstOrDefaultAsync(b => b.Id == boardId, cancellationToken);
        if (board is null)
        {
            return Result<BoardDto>.Failure("Board not found.");
        }

        if (board.OwnerEmployeeId != ownerEmployeeId)
        {
            return Result<BoardDto>.Failure("Only the board owner can remove members.");
        }

        var member = await dbContext.BoardMembers
            .FirstOrDefaultAsync(m => m.BoardId == boardId && m.EmployeeId == employeeId, cancellationToken);
        if (member is null)
        {
            return Result<BoardDto>.Failure("This employee is not a member of the board.");
        }

        dbContext.BoardMembers.Remove(member);

        // Unassign, rather than delete, that member's open tasks so board history is preserved.
        var openTasks = await dbContext.BoardTasks
            .Where(t => t.BoardId == boardId && t.AssigneeEmployeeId == employeeId && t.Status != Domain.Enums.BoardTaskStatus.Done)
            .ToListAsync(cancellationToken);
        foreach (var task in openTasks)
        {
            task.AssigneeEmployeeId = null;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<BoardDto>.Success(await LoadDtoAsync(boardId, cancellationToken));
    }

    internal async Task<(bool IsOwner, bool IsMember)?> CheckAccessAsync(
        Guid boardId, Guid requesterId, CancellationToken cancellationToken)
    {
        var board = await dbContext.Boards.FirstOrDefaultAsync(b => b.Id == boardId, cancellationToken);
        if (board is null)
        {
            return null;
        }

        var isOwner = board.OwnerEmployeeId == requesterId;
        var isMember = isOwner || await dbContext.BoardMembers
            .AnyAsync(m => m.BoardId == boardId && m.EmployeeId == requesterId, cancellationToken);

        return (isOwner, isMember);
    }

    private async Task<BoardDto> LoadDtoAsync(Guid boardId, CancellationToken cancellationToken)
    {
        var board = await dbContext.Boards
            .Include(b => b.OwnerEmployee)
            .Include(b => b.Site)
            .Include(b => b.Members).ThenInclude(m => m.Employee)
            .Include(b => b.Tasks)
            .FirstAsync(b => b.Id == boardId, cancellationToken);

        return ToDto(board);
    }

    private static BoardDto ToDto(Board board) => new(
        board.Id,
        board.Name,
        board.Description,
        board.OwnerEmployeeId,
        board.OwnerEmployee!.FullName,
        board.SiteId,
        board.Site?.Name,
        board.IsArchived,
        board.Priority,
        board.Status,
        board.Deadline,
        ComputeCompletionPercentage(board),
        board.CreatedAtUtc,
        board.Members
            .OrderBy(m => m.Employee!.FirstName)
            .Select(m => new BoardMemberDto(m.EmployeeId, m.Employee!.FullName, m.Employee.JobTitle, m.AddedAtUtc))
            .ToList());

    private static int ComputeCompletionPercentage(Board board)
    {
        if (board.Tasks.Count == 0)
        {
            return 0;
        }

        var done = board.Tasks.Count(t => t.Status == Domain.Enums.BoardTaskStatus.Done);
        return (int)Math.Round(done * 100.0 / board.Tasks.Count);
    }
}
