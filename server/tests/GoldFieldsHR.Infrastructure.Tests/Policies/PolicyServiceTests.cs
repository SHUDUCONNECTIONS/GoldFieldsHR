using GoldFieldsHR.Application.Policies;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Notifications;
using GoldFieldsHR.Infrastructure.Policies;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Policies;

public class PolicyServiceTests
{
    [Fact]
    public async Task Acknowledge_FirstAcknowledgment_ReportsCountOfOne()
    {
        // Regression test: EF Core navigation fixup previously caused the freshly-added
        // acknowledgment to double-count itself in policy.Acknowledgments.Count.
        using var dbContext = TestDbContextFactory.Create();
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = new PolicyService(dbContext, new NotificationService(dbContext));

        var created = await service.CreateAsync(hr.Id, new CreatePolicyRequest("Site Safety Rules", "Wear PPE at all times."));
        Assert.True(created.Succeeded);

        var result = await service.AcknowledgeAsync(created.Value!.Id, employee.Id);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Value!.AcknowledgmentCount);
        Assert.True(result.Value.AcknowledgedByMe);
    }

    [Fact]
    public async Task Acknowledge_SecondEmployee_ReportsCountOfTwo()
    {
        using var dbContext = TestDbContextFactory.Create();
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var employee1 = dbContext.AddEmployee(EmployeeRole.Employee);
        var employee2 = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = new PolicyService(dbContext, new NotificationService(dbContext));

        var created = await service.CreateAsync(hr.Id, new CreatePolicyRequest("Site Safety Rules", "Wear PPE at all times."));
        await service.AcknowledgeAsync(created.Value!.Id, employee1.Id);
        var result = await service.AcknowledgeAsync(created.Value.Id, employee2.Id);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value!.AcknowledgmentCount);
    }

    [Fact]
    public async Task Acknowledge_AlreadyAcknowledged_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = new PolicyService(dbContext, new NotificationService(dbContext));

        var created = await service.CreateAsync(hr.Id, new CreatePolicyRequest("Site Safety Rules", "Wear PPE at all times."));
        await service.AcknowledgeAsync(created.Value!.Id, employee.Id);
        var result = await service.AcknowledgeAsync(created.Value.Id, employee.Id);

        Assert.False(result.Succeeded);
        Assert.Equal("You have already acknowledged this policy.", result.Error);
    }
}
