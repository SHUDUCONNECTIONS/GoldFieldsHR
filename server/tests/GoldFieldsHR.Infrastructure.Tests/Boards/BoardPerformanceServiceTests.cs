using GoldFieldsHR.Application.Boards;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Boards;
using GoldFieldsHR.Infrastructure.Persistence;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Boards;

public class BoardPerformanceServiceTests
{
    private static Board CreateBoard(ApplicationDbContext dbContext, Employee owner)
    {
        var board = new Board { Id = Guid.NewGuid(), Name = "Site Safety Board", OwnerEmployeeId = owner.Id };
        dbContext.Boards.Add(board);
        dbContext.SaveChanges();
        return board;
    }

    [Fact]
    public async Task GetMyPerformance_CountsUnassignedCompletedTask_TowardCreator()
    {
        // Creating and completing a task for yourself without picking yourself as the
        // assignee (the dropdown defaults to "Unassigned") must still count as your work.
        using var dbContext = TestDbContextFactory.Create();
        var owner = dbContext.AddEmployee(EmployeeRole.Employee);
        var board = CreateBoard(dbContext, owner);
        dbContext.BoardTasks.Add(new BoardTask
        {
            Id = Guid.NewGuid(),
            BoardId = board.Id,
            Title = "Sweep the yard",
            AssigneeEmployeeId = null,
            CreatedByEmployeeId = owner.Id,
            Status = BoardTaskStatus.Done,
            CompletedAtUtc = DateTime.UtcNow,
        });
        dbContext.SaveChanges();
        var service = new BoardPerformanceService(dbContext);

        var result = await service.GetMyPerformanceAsync(owner.Id, PerformanceRange.Week);

        Assert.Equal(1, result.TasksCompletedTotal);
    }

    [Fact]
    public async Task GetMyPerformance_DoesNotDoubleCount_WhenAssigneeIsSomeoneElse()
    {
        using var dbContext = TestDbContextFactory.Create();
        var creator = dbContext.AddEmployee(EmployeeRole.LineManager);
        var assignee = dbContext.AddEmployee(EmployeeRole.Employee);
        var board = CreateBoard(dbContext, creator);
        dbContext.BoardTasks.Add(new BoardTask
        {
            Id = Guid.NewGuid(),
            BoardId = board.Id,
            Title = "Inspect conveyor belt",
            AssigneeEmployeeId = assignee.Id,
            CreatedByEmployeeId = creator.Id,
            Status = BoardTaskStatus.Done,
            CompletedAtUtc = DateTime.UtcNow,
        });
        dbContext.SaveChanges();
        var service = new BoardPerformanceService(dbContext);

        var creatorResult = await service.GetMyPerformanceAsync(creator.Id, PerformanceRange.Week);
        var assigneeResult = await service.GetMyPerformanceAsync(assignee.Id, PerformanceRange.Week);

        Assert.Equal(0, creatorResult.TasksCompletedTotal);
        Assert.Equal(1, assigneeResult.TasksCompletedTotal);
    }

    [Fact]
    public async Task GetOrgPerformance_CountsUnassignedCompletedTask_TowardCreator()
    {
        using var dbContext = TestDbContextFactory.Create();
        var owner = dbContext.AddEmployee(EmployeeRole.Employee);
        var board = CreateBoard(dbContext, owner);
        dbContext.BoardTasks.Add(new BoardTask
        {
            Id = Guid.NewGuid(),
            BoardId = board.Id,
            Title = "Sweep the yard",
            AssigneeEmployeeId = null,
            CreatedByEmployeeId = owner.Id,
            Status = BoardTaskStatus.Done,
            CompletedAtUtc = DateTime.UtcNow,
        });
        dbContext.SaveChanges();
        var service = new BoardPerformanceService(dbContext);

        var result = await service.GetOrgPerformanceAsync(null, PerformanceRange.Week);

        var ownerRow = Assert.Single(result, r => r.EmployeeId == owner.Id);
        Assert.Equal(1, ownerRow.TasksCompleted);
    }
}
