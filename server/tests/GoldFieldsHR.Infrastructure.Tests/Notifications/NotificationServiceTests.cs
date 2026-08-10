using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Notifications;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Notifications;

public class NotificationServiceTests
{
    [Fact]
    public async Task CreateAsync_ThenGetMine_ReturnsIt()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = new NotificationService(dbContext);

        await service.CreateAsync(employee.Id, "Your leave request was approved.", "/leave");

        var mine = await service.GetMineAsync(employee.Id);

        Assert.Single(mine);
        Assert.Equal("Your leave request was approved.", mine[0].Message);
        Assert.False(mine[0].IsRead);
    }

    [Fact]
    public async Task CreateForManyAsync_NotifiesEveryRecipient()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee1 = dbContext.AddEmployee(EmployeeRole.Employee);
        var employee2 = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = new NotificationService(dbContext);

        await service.CreateForManyAsync([employee1.Id, employee2.Id], "New policy published.", "/policies");

        Assert.Single(await service.GetMineAsync(employee1.Id));
        Assert.Single(await service.GetMineAsync(employee2.Id));
    }

    [Fact]
    public async Task GetUnreadCount_OnlyCountsUnread()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = new NotificationService(dbContext);

        await service.CreateAsync(employee.Id, "First", null);
        await service.CreateAsync(employee.Id, "Second", null);

        Assert.Equal(2, await service.GetUnreadCountAsync(employee.Id));

        var mine = await service.GetMineAsync(employee.Id);
        await service.MarkAsReadAsync(mine[0].Id, employee.Id);

        Assert.Equal(1, await service.GetUnreadCountAsync(employee.Id));
    }

    [Fact]
    public async Task MarkAsRead_WrongEmployee_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var stranger = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = new NotificationService(dbContext);

        await service.CreateAsync(employee.Id, "Private message", null);
        var mine = await service.GetMineAsync(employee.Id);

        var result = await service.MarkAsReadAsync(mine[0].Id, stranger.Id);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task MarkAllAsRead_ClearsEveryUnreadNotification()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = new NotificationService(dbContext);

        await service.CreateAsync(employee.Id, "First", null);
        await service.CreateAsync(employee.Id, "Second", null);

        await service.MarkAllAsReadAsync(employee.Id);

        Assert.Equal(0, await service.GetUnreadCountAsync(employee.Id));
    }
}
