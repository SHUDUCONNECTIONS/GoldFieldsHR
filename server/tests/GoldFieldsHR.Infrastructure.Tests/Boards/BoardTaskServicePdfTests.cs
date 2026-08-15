using System.Text;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Boards;
using GoldFieldsHR.Infrastructure.Notifications;
using GoldFieldsHR.Infrastructure.Persistence;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Boards;

public class BoardTaskServicePdfTests
{
    private static Board CreateBoardWithMember(ApplicationDbContext dbContext, Employee owner, Employee member)
    {
        var board = new Board { Id = Guid.NewGuid(), Name = "Site Safety Board", OwnerEmployeeId = owner.Id };
        board.Members.Add(new BoardMember { Id = Guid.NewGuid(), BoardId = board.Id, EmployeeId = owner.Id });
        board.Members.Add(new BoardMember { Id = Guid.NewGuid(), BoardId = board.Id, EmployeeId = member.Id });
        dbContext.Boards.Add(board);
        dbContext.SaveChanges();
        return board;
    }

    [Fact]
    public async Task GenerateWeeklySummaryPdf_ForOwner_ReturnsValidPdfBytes()
    {
        using var dbContext = TestDbContextFactory.Create();
        var owner = dbContext.AddEmployee(EmployeeRole.LineManager);
        var member = dbContext.AddEmployee(EmployeeRole.Employee);
        var board = CreateBoardWithMember(dbContext, owner, member);
        var service = new BoardTaskService(dbContext, new NotificationService(dbContext));

        var result = await service.GenerateWeeklySummaryPdfAsync(board.Id, owner.Id, null);

        Assert.True(result.Succeeded);
        Assert.NotEmpty(result.Value!);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(result.Value!, 0, 4));
    }

    [Fact]
    public async Task GenerateWeeklySummaryPdf_ForNonOwner_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var owner = dbContext.AddEmployee(EmployeeRole.LineManager);
        var member = dbContext.AddEmployee(EmployeeRole.Employee);
        var board = CreateBoardWithMember(dbContext, owner, member);
        var service = new BoardTaskService(dbContext, new NotificationService(dbContext));

        var result = await service.GenerateWeeklySummaryPdfAsync(board.Id, member.Id, null);

        Assert.False(result.Succeeded);
    }
}
