using GoldFieldsHR.Application.Emergency;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Emergency;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Emergency;

public class EmergencyServiceTests
{
    [Fact]
    public async Task Trigger_WhileAlreadyActive_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = new EmergencyService(dbContext);

        var first = await service.TriggerAsync(employee.Id, new TriggerEmergencyAlertRequest("Shaft 3", "Help"));
        Assert.True(first.Succeeded);

        var second = await service.TriggerAsync(employee.Id, new TriggerEmergencyAlertRequest("Shaft 3", null));

        Assert.False(second.Succeeded);
        Assert.Equal("You already have an active SOS alert.", second.Error);
    }

    [Fact]
    public async Task Trigger_AfterPriorAlertResolved_Succeeds()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var security = dbContext.AddEmployee(EmployeeRole.Security);
        var service = new EmergencyService(dbContext);

        var first = await service.TriggerAsync(employee.Id, new TriggerEmergencyAlertRequest("Shaft 3", null));
        await service.ResolveAsync(first.Value!.Id, security.Id, new ResolveEmergencyAlertRequest("False alarm"));

        var second = await service.TriggerAsync(employee.Id, new TriggerEmergencyAlertRequest("Surface", null));

        Assert.True(second.Succeeded);
    }

    [Fact]
    public async Task Resolve_AlreadyResolved_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var security = dbContext.AddEmployee(EmployeeRole.Security);
        var service = new EmergencyService(dbContext);

        var alert = await service.TriggerAsync(employee.Id, new TriggerEmergencyAlertRequest("Shaft 3", null));
        await service.ResolveAsync(alert.Value!.Id, security.Id, new ResolveEmergencyAlertRequest(null));

        var result = await service.ResolveAsync(alert.Value.Id, security.Id, new ResolveEmergencyAlertRequest(null));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task GetActiveAlerts_OnlyReturnsActiveOnes()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee1 = dbContext.AddEmployee(EmployeeRole.Employee);
        var employee2 = dbContext.AddEmployee(EmployeeRole.Employee);
        var security = dbContext.AddEmployee(EmployeeRole.Security);
        var service = new EmergencyService(dbContext);

        var alert1 = await service.TriggerAsync(employee1.Id, new TriggerEmergencyAlertRequest("Shaft 3", null));
        await service.TriggerAsync(employee2.Id, new TriggerEmergencyAlertRequest("Surface", null));
        await service.ResolveAsync(alert1.Value!.Id, security.Id, new ResolveEmergencyAlertRequest(null));

        var active = await service.GetActiveAlertsAsync();

        Assert.Single(active);
        Assert.Equal(EmergencyAlertStatus.Active, active[0].Status);
    }
}
