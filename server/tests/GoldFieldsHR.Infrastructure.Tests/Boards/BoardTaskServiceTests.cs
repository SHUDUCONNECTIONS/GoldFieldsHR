using GoldFieldsHR.Application.Boards;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Boards;
using GoldFieldsHR.Infrastructure.Notifications;
using GoldFieldsHR.Infrastructure.Persistence;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Boards;

public class BoardTaskServiceTests
{
    private static (Board board, BoardTask task) CreateBoardWithTask(
        ApplicationDbContext dbContext, Employee owner, Employee assignee)
    {
        var board = new Board { Id = Guid.NewGuid(), Name = "Site Safety Board", OwnerEmployeeId = owner.Id };
        board.Members.Add(new BoardMember { Id = Guid.NewGuid(), BoardId = board.Id, EmployeeId = owner.Id });
        board.Members.Add(new BoardMember { Id = Guid.NewGuid(), BoardId = board.Id, EmployeeId = assignee.Id });
        dbContext.Boards.Add(board);

        var task = new BoardTask
        {
            Id = Guid.NewGuid(),
            BoardId = board.Id,
            Title = "Inspect conveyor belt",
            AssigneeEmployeeId = assignee.Id,
            CreatedByEmployeeId = owner.Id,
        };
        dbContext.BoardTasks.Add(task);
        dbContext.SaveChanges();

        return (board, task);
    }

    [Fact]
    public async Task ChangeStatus_ToDone_WithoutAttachment_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var owner = dbContext.AddEmployee(EmployeeRole.LineManager);
        var assignee = dbContext.AddEmployee(EmployeeRole.Employee);
        var (board, task) = CreateBoardWithTask(dbContext, owner, assignee);
        var service = new BoardTaskService(dbContext, new NotificationService(dbContext));

        var result = await service.ChangeStatusAsync(
            board.Id, task.Id, assignee.Id, new ChangeTaskStatusRequest(BoardTaskStatus.Done));

        Assert.False(result.Succeeded);
        Assert.Contains("proof of work", result.Error);
    }

    [Fact]
    public async Task ChangeStatus_ToDone_WithAttachment_SucceedsAndSetsCompletedAtUtc()
    {
        using var dbContext = TestDbContextFactory.Create();
        var owner = dbContext.AddEmployee(EmployeeRole.LineManager);
        var assignee = dbContext.AddEmployee(EmployeeRole.Employee);
        var (board, task) = CreateBoardWithTask(dbContext, owner, assignee);
        dbContext.Attachments.Add(new Attachment
        {
            Id = Guid.NewGuid(),
            EntityType = AttachmentEntityType.BoardTask,
            EntityId = task.Id,
            FileName = "proof.jpg",
            StoredFileName = "stored.jpg",
            ContentType = "image/jpeg",
            SizeBytes = 10,
            UploadedByEmployeeId = assignee.Id,
        });
        dbContext.SaveChanges();
        var service = new BoardTaskService(dbContext, new NotificationService(dbContext));

        var result = await service.ChangeStatusAsync(
            board.Id, task.Id, assignee.Id, new ChangeTaskStatusRequest(BoardTaskStatus.Done));

        Assert.True(result.Succeeded);
        Assert.Equal(BoardTaskStatus.Done, result.Value!.Status);
        Assert.NotNull(result.Value.CompletedAtUtc);
    }

    [Fact]
    public async Task ChangeStatus_ByNonOwnerNonAssignee_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var owner = dbContext.AddEmployee(EmployeeRole.LineManager);
        var assignee = dbContext.AddEmployee(EmployeeRole.Employee);
        var stranger = dbContext.AddEmployee(EmployeeRole.Employee);
        var (board, task) = CreateBoardWithTask(dbContext, owner, assignee);
        var service = new BoardTaskService(dbContext, new NotificationService(dbContext));

        var result = await service.ChangeStatusAsync(
            board.Id, task.Id, stranger.Id, new ChangeTaskStatusRequest(BoardTaskStatus.InProgress));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ChangeStatus_MovedOutOfDone_ClearsCompletedAtUtc()
    {
        using var dbContext = TestDbContextFactory.Create();
        var owner = dbContext.AddEmployee(EmployeeRole.LineManager);
        var assignee = dbContext.AddEmployee(EmployeeRole.Employee);
        var (board, task) = CreateBoardWithTask(dbContext, owner, assignee);
        dbContext.Attachments.Add(new Attachment
        {
            Id = Guid.NewGuid(),
            EntityType = AttachmentEntityType.BoardTask,
            EntityId = task.Id,
            FileName = "proof.jpg",
            StoredFileName = "stored.jpg",
            ContentType = "image/jpeg",
            SizeBytes = 10,
            UploadedByEmployeeId = assignee.Id,
        });
        dbContext.SaveChanges();
        var service = new BoardTaskService(dbContext, new NotificationService(dbContext));
        await service.ChangeStatusAsync(board.Id, task.Id, assignee.Id, new ChangeTaskStatusRequest(BoardTaskStatus.Done));

        var result = await service.ChangeStatusAsync(
            board.Id, task.Id, assignee.Id, new ChangeTaskStatusRequest(BoardTaskStatus.InProgress));

        Assert.True(result.Succeeded);
        Assert.Null(result.Value!.CompletedAtUtc);
    }
}
