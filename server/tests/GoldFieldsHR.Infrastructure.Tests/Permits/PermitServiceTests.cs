using GoldFieldsHR.Application.Permits;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Notifications;
using GoldFieldsHR.Infrastructure.Permits;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Permits;

public class PermitServiceTests
{
    [Fact]
    public async Task Submit_ValidToBeforeValidFrom_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = new PermitService(dbContext, new NotificationService(dbContext));

        var result = await service.SubmitAsync(employee.Id, new SubmitPermitRequest(
            PermitType.HotWork, "Shaft 3", "Welding repair",
            new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 5)));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task FullWorkflow_SubmitApproveClose_MovesThroughStatusesCorrectly()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var safetyOfficer = dbContext.AddEmployee(EmployeeRole.SafetyOfficer);
        var service = new PermitService(dbContext, new NotificationService(dbContext));

        var submitted = await service.SubmitAsync(employee.Id, new SubmitPermitRequest(
            PermitType.ConfinedSpace, "Tank 2", "Inspection", new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 11)));
        Assert.Equal(PermitStatus.Pending, submitted.Value!.Status);

        var approved = await service.ReviewAsync(submitted.Value.Id, safetyOfficer.Id, new ReviewPermitRequest(true, null));
        Assert.Equal(PermitStatus.Approved, approved.Value!.Status);

        var open = await service.GetOpenPermitsAsync();
        Assert.Single(open);

        var closed = await service.CloseAsync(submitted.Value.Id, new ClosePermitRequest("Work complete"));
        Assert.True(closed.Succeeded);
        Assert.Equal(PermitStatus.Closed, closed.Value!.Status);
        Assert.NotNull(closed.Value.ClosedAtUtc);

        Assert.Empty(await service.GetOpenPermitsAsync());
    }

    [Fact]
    public async Task Close_RejectedPermit_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var safetyOfficer = dbContext.AddEmployee(EmployeeRole.SafetyOfficer);
        var service = new PermitService(dbContext, new NotificationService(dbContext));

        var submitted = await service.SubmitAsync(employee.Id, new SubmitPermitRequest(
            PermitType.Excavation, "Pit 1", "Trenching", new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 11)));
        await service.ReviewAsync(submitted.Value!.Id, safetyOfficer.Id, new ReviewPermitRequest(false, "Missing plan"));

        var result = await service.CloseAsync(submitted.Value.Id, new ClosePermitRequest(null));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Close_AlreadyClosed_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var safetyOfficer = dbContext.AddEmployee(EmployeeRole.SafetyOfficer);
        var service = new PermitService(dbContext, new NotificationService(dbContext));

        var submitted = await service.SubmitAsync(employee.Id, new SubmitPermitRequest(
            PermitType.WorkingAtHeight, "Headgear", "Maintenance", new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 11)));
        await service.ReviewAsync(submitted.Value!.Id, safetyOfficer.Id, new ReviewPermitRequest(true, null));
        await service.CloseAsync(submitted.Value.Id, new ClosePermitRequest(null));

        var result = await service.CloseAsync(submitted.Value.Id, new ClosePermitRequest(null));

        Assert.False(result.Succeeded);
    }
}
