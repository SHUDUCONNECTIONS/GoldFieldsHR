using GoldFieldsHR.Application.Boards;
using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Notifications;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace GoldFieldsHR.Infrastructure.Boards;

public class BoardTaskService(ApplicationDbContext dbContext, INotificationService notificationService) : IBoardTaskService
{
    public async Task<Result<BoardTaskDto>> CreateAsync(
        Guid boardId, Guid creatorEmployeeId, CreateBoardTaskRequest request, CancellationToken cancellationToken = default)
    {
        var board = await dbContext.Boards.FirstOrDefaultAsync(b => b.Id == boardId, cancellationToken);
        if (board is null)
        {
            return Result<BoardTaskDto>.Failure("Board not found.");
        }

        if (board.OwnerEmployeeId != creatorEmployeeId)
        {
            return Result<BoardTaskDto>.Failure("Only the board owner can create tasks.");
        }

        if (request.AssigneeEmployeeId.HasValue)
        {
            var isMember = await dbContext.BoardMembers
                .AnyAsync(m => m.BoardId == boardId && m.EmployeeId == request.AssigneeEmployeeId.Value, cancellationToken);
            if (!isMember)
            {
                return Result<BoardTaskDto>.Failure("The assignee must be a member of this board.");
            }
        }

        var task = new BoardTask
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            Title = request.Title,
            Description = request.Description,
            AssigneeEmployeeId = request.AssigneeEmployeeId,
            CreatedByEmployeeId = creatorEmployeeId,
            DueDate = request.DueDate,
        };

        dbContext.BoardTasks.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (request.AssigneeEmployeeId.HasValue)
        {
            await notificationService.CreateAsync(
                request.AssigneeEmployeeId.Value,
                $"You were assigned a new task: \"{task.Title}\".",
                $"/boards/{boardId}/tasks/{task.Id}",
                cancellationToken);
        }

        return Result<BoardTaskDto>.Success(await LoadDtoAsync(task.Id, cancellationToken));
    }

    public async Task<Result<IReadOnlyList<BoardTaskDto>>> GetForBoardAsync(
        Guid boardId, Guid requesterId, BoardTaskStatus? status, Guid? assigneeId,
        CancellationToken cancellationToken = default)
    {
        var access = await CheckAccessAsync(boardId, requesterId, cancellationToken);
        if (access is null)
        {
            return Result<IReadOnlyList<BoardTaskDto>>.Failure("Board not found.");
        }

        if (!access.Value)
        {
            return Result<IReadOnlyList<BoardTaskDto>>.Failure("You are not a member of this board.");
        }

        var query = dbContext.BoardTasks
            .Include(t => t.AssigneeEmployee)
            .Include(t => t.CreatedByEmployee)
            .Where(t => t.BoardId == boardId);

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (assigneeId.HasValue)
        {
            query = query.Where(t => t.AssigneeEmployeeId == assigneeId.Value);
        }

        var tasks = await query.OrderBy(t => t.CreatedAtUtc).ToListAsync(cancellationToken);

        return Result<IReadOnlyList<BoardTaskDto>>.Success(tasks.Select(ToDto).ToList());
    }

    public async Task<Result<BoardTaskDto>> GetByIdAsync(
        Guid boardId, Guid taskId, Guid requesterId, CancellationToken cancellationToken = default)
    {
        var access = await CheckAccessAsync(boardId, requesterId, cancellationToken);
        if (access is null)
        {
            return Result<BoardTaskDto>.Failure("Board not found.");
        }

        if (!access.Value)
        {
            return Result<BoardTaskDto>.Failure("You are not a member of this board.");
        }

        var exists = await dbContext.BoardTasks.AnyAsync(t => t.Id == taskId && t.BoardId == boardId, cancellationToken);
        if (!exists)
        {
            return Result<BoardTaskDto>.Failure("Task not found.");
        }

        return Result<BoardTaskDto>.Success(await LoadDtoAsync(taskId, cancellationToken));
    }

    public async Task<Result<BoardTaskDto>> UpdateAsync(
        Guid boardId, Guid taskId, Guid ownerEmployeeId, UpdateBoardTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var board = await dbContext.Boards.FirstOrDefaultAsync(b => b.Id == boardId, cancellationToken);
        if (board is null)
        {
            return Result<BoardTaskDto>.Failure("Board not found.");
        }

        if (board.OwnerEmployeeId != ownerEmployeeId)
        {
            return Result<BoardTaskDto>.Failure("Only the board owner can edit tasks.");
        }

        var task = await dbContext.BoardTasks.FirstOrDefaultAsync(t => t.Id == taskId && t.BoardId == boardId, cancellationToken);
        if (task is null)
        {
            return Result<BoardTaskDto>.Failure("Task not found.");
        }

        if (request.AssigneeEmployeeId.HasValue)
        {
            var isMember = await dbContext.BoardMembers
                .AnyAsync(m => m.BoardId == boardId && m.EmployeeId == request.AssigneeEmployeeId.Value, cancellationToken);
            if (!isMember)
            {
                return Result<BoardTaskDto>.Failure("The assignee must be a member of this board.");
            }
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.AssigneeEmployeeId = request.AssigneeEmployeeId;
        task.DueDate = request.DueDate;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<BoardTaskDto>.Success(await LoadDtoAsync(taskId, cancellationToken));
    }

    public async Task<Result<BoardTaskDto>> ChangeStatusAsync(
        Guid boardId, Guid taskId, Guid requesterId, ChangeTaskStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var board = await dbContext.Boards.FirstOrDefaultAsync(b => b.Id == boardId, cancellationToken);
        if (board is null)
        {
            return Result<BoardTaskDto>.Failure("Board not found.");
        }

        var task = await dbContext.BoardTasks.FirstOrDefaultAsync(t => t.Id == taskId && t.BoardId == boardId, cancellationToken);
        if (task is null)
        {
            return Result<BoardTaskDto>.Failure("Task not found.");
        }

        var isOwner = board.OwnerEmployeeId == requesterId;
        var isAssignee = task.AssigneeEmployeeId == requesterId;
        if (!isOwner && !isAssignee)
        {
            return Result<BoardTaskDto>.Failure("Only the board owner or the assignee can change this task's status.");
        }

        if (request.Status == BoardTaskStatus.Done)
        {
            var hasAttachment = await dbContext.Attachments.AnyAsync(
                a => a.EntityType == AttachmentEntityType.BoardTask && a.EntityId == taskId, cancellationToken);
            if (!hasAttachment)
            {
                return Result<BoardTaskDto>.Failure("Attach proof of work before marking this task done.");
            }
        }

        task.Status = request.Status;
        task.CompletedAtUtc = request.Status == BoardTaskStatus.Done ? DateTime.UtcNow : null;
        await dbContext.SaveChangesAsync(cancellationToken);

        if (isAssignee && !isOwner)
        {
            await notificationService.CreateAsync(
                board.OwnerEmployeeId,
                $"\"{task.Title}\" was moved to {task.Status}.",
                $"/boards/{boardId}/tasks/{taskId}",
                cancellationToken);
        }
        else if (isOwner && task.AssigneeEmployeeId.HasValue && task.AssigneeEmployeeId != requesterId)
        {
            await notificationService.CreateAsync(
                task.AssigneeEmployeeId.Value,
                $"\"{task.Title}\" was moved to {task.Status}.",
                $"/boards/{boardId}/tasks/{taskId}",
                cancellationToken);
        }

        return Result<BoardTaskDto>.Success(await LoadDtoAsync(taskId, cancellationToken));
    }

    public async Task<Result<bool>> DeleteAsync(
        Guid boardId, Guid taskId, Guid ownerEmployeeId, CancellationToken cancellationToken = default)
    {
        var board = await dbContext.Boards.FirstOrDefaultAsync(b => b.Id == boardId, cancellationToken);
        if (board is null)
        {
            return Result<bool>.Failure("Board not found.");
        }

        if (board.OwnerEmployeeId != ownerEmployeeId)
        {
            return Result<bool>.Failure("Only the board owner can delete tasks.");
        }

        var task = await dbContext.BoardTasks.FirstOrDefaultAsync(t => t.Id == taskId && t.BoardId == boardId, cancellationToken);
        if (task is null)
        {
            return Result<bool>.Failure("Task not found.");
        }

        dbContext.BoardTasks.Remove(task);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<IReadOnlyList<WeeklyTaskCompletionDto>>> GetWeeklySummaryAsync(
        Guid boardId, Guid ownerEmployeeId, DateOnly? weekStartUtc, CancellationToken cancellationToken = default)
    {
        var computed = await ComputeWeeklySummaryAsync(boardId, ownerEmployeeId, weekStartUtc, cancellationToken);
        return computed.Succeeded
            ? Result<IReadOnlyList<WeeklyTaskCompletionDto>>.Success(computed.Value!.Rows)
            : Result<IReadOnlyList<WeeklyTaskCompletionDto>>.Failure(computed.Error!);
    }

    public async Task<Result<byte[]>> GenerateWeeklySummaryPdfAsync(
        Guid boardId, Guid ownerEmployeeId, DateOnly? weekStartUtc, CancellationToken cancellationToken = default)
    {
        var computed = await ComputeWeeklySummaryAsync(boardId, ownerEmployeeId, weekStartUtc, cancellationToken);
        if (!computed.Succeeded)
        {
            return Result<byte[]>.Failure(computed.Error!);
        }

        var pdfBytes = BuildWeeklySummaryPdf(computed.Value!.Board, computed.Value.WeekStart, computed.Value.Rows);
        return Result<byte[]>.Success(pdfBytes);
    }

    private async Task<Result<(Board Board, DateOnly WeekStart, IReadOnlyList<WeeklyTaskCompletionDto> Rows)>> ComputeWeeklySummaryAsync(
        Guid boardId, Guid ownerEmployeeId, DateOnly? weekStartUtc, CancellationToken cancellationToken)
    {
        var board = await dbContext.Boards
            .Include(b => b.Members).ThenInclude(m => m.Employee)
            .FirstOrDefaultAsync(b => b.Id == boardId, cancellationToken);
        if (board is null)
        {
            return Result<(Board, DateOnly, IReadOnlyList<WeeklyTaskCompletionDto>)>.Failure("Board not found.");
        }

        if (board.OwnerEmployeeId != ownerEmployeeId)
        {
            return Result<(Board, DateOnly, IReadOnlyList<WeeklyTaskCompletionDto>)>.Failure(
                "Only the board owner can view the weekly summary.");
        }

        var weekStart = weekStartUtc ?? GetIsoWeekStart(DateOnly.FromDateTime(DateTime.UtcNow));
        var weekStartDateTime = weekStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var weekEndDateTime = weekStartDateTime.AddDays(7);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var tasks = await dbContext.BoardTasks
            .Where(t => t.BoardId == boardId && t.AssigneeEmployeeId != null)
            .ToListAsync(cancellationToken);

        var summary = board.Members
            .OrderBy(m => m.Employee!.FirstName)
            .Select(member =>
            {
                var memberTasks = tasks.Where(t => t.AssigneeEmployeeId == member.EmployeeId).ToList();
                var completed = memberTasks.Count(t =>
                    t.CompletedAtUtc.HasValue && t.CompletedAtUtc.Value >= weekStartDateTime && t.CompletedAtUtc.Value < weekEndDateTime);
                var inProgress = memberTasks.Count(t => t.Status == BoardTaskStatus.InProgress);
                var overdue = memberTasks.Count(t => t.Status != BoardTaskStatus.Done && t.DueDate.HasValue && t.DueDate.Value < today);

                return new WeeklyTaskCompletionDto(member.EmployeeId, member.Employee!.FullName, completed, inProgress, overdue);
            })
            .ToList();

        return Result<(Board, DateOnly, IReadOnlyList<WeeklyTaskCompletionDto>)>.Success((board, weekStart, summary));
    }

    private static byte[] BuildWeeklySummaryPdf(Board board, DateOnly weekStart, IReadOnlyList<WeeklyTaskCompletionDto> rows)
    {
        var weekEnd = weekStart.AddDays(6);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(column =>
                {
                    column.Item().Text(board.Name).FontSize(18).Bold();
                    column.Item().Text(
                        $"Weekly Performance Summary: {weekStart:d MMM yyyy} - {weekEnd:d MMM yyyy}")
                        .FontSize(12).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Employee").Bold();
                        header.Cell().Text("Completed").Bold();
                        header.Cell().Text("In Progress").Bold();
                        header.Cell().Text("Overdue").Bold();
                    });

                    foreach (var row in rows)
                    {
                        table.Cell().Text(row.EmployeeName);
                        table.Cell().Text(row.TasksCompleted.ToString());
                        table.Cell().Text(row.TasksInProgress.ToString());
                        table.Cell().Text(row.TasksOverdue.ToString());
                    }
                });

                page.Footer().AlignCenter().Text(
                    $"Generated {DateTime.UtcNow:d MMM yyyy HH:mm} UTC").FontSize(9).FontColor(Colors.Grey.Medium);
            });
        }).GeneratePdf();
    }

    private static DateOnly GetIsoWeekStart(DateOnly date)
    {
        var diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-diff);
    }

    private async Task<bool?> CheckAccessAsync(Guid boardId, Guid requesterId, CancellationToken cancellationToken)
    {
        var board = await dbContext.Boards.FirstOrDefaultAsync(b => b.Id == boardId, cancellationToken);
        if (board is null)
        {
            return null;
        }

        if (board.OwnerEmployeeId == requesterId)
        {
            return true;
        }

        return await dbContext.BoardMembers.AnyAsync(m => m.BoardId == boardId && m.EmployeeId == requesterId, cancellationToken);
    }

    private async Task<BoardTaskDto> LoadDtoAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var task = await dbContext.BoardTasks
            .Include(t => t.AssigneeEmployee)
            .Include(t => t.CreatedByEmployee)
            .FirstAsync(t => t.Id == taskId, cancellationToken);

        return ToDto(task);
    }

    private static BoardTaskDto ToDto(BoardTask task) => new(
        task.Id,
        task.BoardId,
        task.Title,
        task.Description,
        task.AssigneeEmployeeId,
        task.AssigneeEmployee?.FullName,
        task.CreatedByEmployeeId,
        task.CreatedByEmployee!.FullName,
        task.Status,
        task.DueDate,
        task.CreatedAtUtc,
        task.CompletedAtUtc);
}
