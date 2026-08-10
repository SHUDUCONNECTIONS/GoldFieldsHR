using GoldFieldsHR.Application.Performance;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Performance;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Performance;

public class PerformanceServiceTests
{
    [Fact]
    public async Task Create_ReviewingSelf_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var manager = dbContext.AddEmployee(EmployeeRole.LineManager, "LM-1");
        var service = new PerformanceService(dbContext);

        var result = await service.CreateAsync(manager.Id, new CreatePerformanceReviewRequest("LM-1", "Q1 2026", 5, null));

        Assert.False(result.Succeeded);
        Assert.Equal("You cannot review yourself.", result.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task Create_ScoreOutOfRange_Fails(int score)
    {
        using var dbContext = TestDbContextFactory.Create();
        var manager = dbContext.AddEmployee(EmployeeRole.LineManager);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee, "EMP-1");
        var service = new PerformanceService(dbContext);

        var result = await service.CreateAsync(manager.Id, new CreatePerformanceReviewRequest("EMP-1", "Q1 2026", score, null));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Create_ValidReview_AppearsInBothMineAndGiven()
    {
        using var dbContext = TestDbContextFactory.Create();
        var manager = dbContext.AddEmployee(EmployeeRole.LineManager);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee, "EMP-1");
        var service = new PerformanceService(dbContext);

        var created = await service.CreateAsync(manager.Id, new CreatePerformanceReviewRequest("EMP-1", "Q1 2026", 4, "Good work"));
        Assert.True(created.Succeeded);

        var mine = await service.GetMyReviewsAsync(employee.Id);
        var given = await service.GetGivenByMeAsync(manager.Id);

        Assert.Single(mine);
        Assert.Single(given);
        Assert.Equal(4, mine[0].Score);
    }

    [Fact]
    public async Task Create_UnknownEmployeeNumber_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var manager = dbContext.AddEmployee(EmployeeRole.LineManager);
        var service = new PerformanceService(dbContext);

        var result = await service.CreateAsync(manager.Id, new CreatePerformanceReviewRequest("DOES-NOT-EXIST", "Q1 2026", 3, null));

        Assert.False(result.Succeeded);
    }
}
