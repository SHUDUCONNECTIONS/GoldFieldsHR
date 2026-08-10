using GoldFieldsHR.Application.Ppe;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Notifications;
using GoldFieldsHR.Infrastructure.Ppe;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Ppe;

public class PpeServiceTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public async Task Submit_QuantityOutOfRange_Fails(int quantity)
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = new PpeService(dbContext, new NotificationService(dbContext));

        var result = await service.SubmitAsync(employee.Id, new SubmitPpeRequest(PpeItemType.Helmet, null, quantity, "New starter"));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task FullWorkflow_SubmitApproveIssue_MovesThroughStatusesCorrectly()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var safetyOfficer = dbContext.AddEmployee(EmployeeRole.SafetyOfficer);
        var service = new PpeService(dbContext, new NotificationService(dbContext));

        var submitted = await service.SubmitAsync(employee.Id, new SubmitPpeRequest(PpeItemType.SafetyBoots, "9", 1, "Worn out"));
        Assert.Equal(PpeRequestStatus.Pending, submitted.Value!.Status);

        var pending = await service.GetPendingApprovalsAsync();
        Assert.Single(pending);

        var approved = await service.ReviewAsync(submitted.Value.Id, safetyOfficer.Id, new ReviewPpeRequest(true, null));
        Assert.True(approved.Succeeded);
        Assert.Equal(PpeRequestStatus.Approved, approved.Value!.Status);

        var awaitingIssue = await service.GetAwaitingIssueAsync();
        Assert.Single(awaitingIssue);

        var issued = await service.MarkIssuedAsync(submitted.Value.Id, safetyOfficer.Id);
        Assert.True(issued.Succeeded);
        Assert.Equal(PpeRequestStatus.Issued, issued.Value!.Status);
        Assert.NotNull(issued.Value.IssuedAtUtc);

        Assert.Empty(await service.GetAwaitingIssueAsync());
    }

    [Fact]
    public async Task Review_AlreadyReviewed_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var safetyOfficer = dbContext.AddEmployee(EmployeeRole.SafetyOfficer);
        var service = new PpeService(dbContext, new NotificationService(dbContext));

        var submitted = await service.SubmitAsync(employee.Id, new SubmitPpeRequest(PpeItemType.Gloves, null, 2, "Replacement"));
        await service.ReviewAsync(submitted.Value!.Id, safetyOfficer.Id, new ReviewPpeRequest(false, "Not due"));

        var result = await service.ReviewAsync(submitted.Value.Id, safetyOfficer.Id, new ReviewPpeRequest(true, null));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task MarkIssued_NotApproved_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var safetyOfficer = dbContext.AddEmployee(EmployeeRole.SafetyOfficer);
        var service = new PpeService(dbContext, new NotificationService(dbContext));

        var submitted = await service.SubmitAsync(employee.Id, new SubmitPpeRequest(PpeItemType.Helmet, null, 1, "New starter"));

        var result = await service.MarkIssuedAsync(submitted.Value!.Id, safetyOfficer.Id);

        Assert.False(result.Succeeded);
    }
}
