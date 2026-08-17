using GoldFieldsHR.Application.Leave;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Leave;
using GoldFieldsHR.Infrastructure.Notifications;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Leave;

public class LeaveServiceTests
{
    // 1x1 transparent PNG, used wherever a test needs a stand-in signature image.
    private const string SamplePngBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=";

    [Fact]
    public async Task GetPendingLineManagerApprovals_DirectReportsSurfaceFirst_ButOthersStillIncluded()
    {
        using var dbContext = TestDbContextFactory.Create();
        var manager = dbContext.AddEmployee(EmployeeRole.LineManager);
        var otherManager = dbContext.AddEmployee(EmployeeRole.LineManager);
        var directReport = dbContext.AddEmployee(EmployeeRole.Employee);
        directReport.ManagerId = manager.Id;
        var unrelatedEmployee = dbContext.AddEmployee(EmployeeRole.Employee);
        dbContext.SaveChanges();

        var service = new LeaveService(dbContext, new NotificationService(dbContext));
        await service.SubmitAsync(unrelatedEmployee.Id, new SubmitLeaveRequest(
            LeaveType.Annual, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 3), "Trip", "0820000000"));
        await service.SubmitAsync(directReport.Id, new SubmitLeaveRequest(
            LeaveType.Sick, new DateOnly(2026, 9, 5), new DateOnly(2026, 9, 5), "Not well", "0820000001"));

        var pending = await service.GetPendingLineManagerApprovalsAsync(manager.Id);

        Assert.Equal(2, pending.Count);
        Assert.True(pending[0].IsDirectReport);
        Assert.Equal(directReport.Id, pending[0].EmployeeId);
        Assert.False(pending[1].IsDirectReport);
    }

    [Fact]
    public async Task GetPendingLineManagerApprovals_NoDirectReports_StillReturnsSiteWideFallback()
    {
        using var dbContext = TestDbContextFactory.Create();
        var manager = dbContext.AddEmployee(EmployeeRole.LineManager);
        var unrelatedEmployee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = new LeaveService(dbContext, new NotificationService(dbContext));

        await service.SubmitAsync(unrelatedEmployee.Id, new SubmitLeaveRequest(
            LeaveType.Annual, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 3), "Trip", "0820000000"));

        var pending = await service.GetPendingLineManagerApprovalsAsync(manager.Id);

        Assert.Single(pending);
        Assert.False(pending[0].IsDirectReport);
    }

    [Fact]
    public async Task LineManagerReviewAsync_Approve_MovesToPendingHRAndNotifiesTheRequester()
    {
        using var dbContext = TestDbContextFactory.Create();
        var manager = dbContext.AddEmployee(EmployeeRole.LineManager);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var notificationService = new NotificationService(dbContext);
        var service = new LeaveService(dbContext, notificationService);

        var submitted = await service.SubmitAsync(employee.Id, new SubmitLeaveRequest(
            LeaveType.Annual, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 3), "Trip", "0820000000"));
        var reviewed = await service.LineManagerReviewAsync(
            submitted.Value!.Id, manager.Id, new ReviewLeaveRequest(true, null, SamplePngBase64));

        Assert.True(reviewed.Succeeded);
        Assert.Equal(LeaveRequestStatus.PendingHRApproval, reviewed.Value!.Status);

        var notifications = await notificationService.GetMineAsync(employee.Id);

        Assert.Single(notifications);
        Assert.Contains("approved", notifications[0].Message);
    }

    [Fact]
    public async Task HRReviewAsync_Approve_MarksFullyApproved()
    {
        using var dbContext = TestDbContextFactory.Create();
        var manager = dbContext.AddEmployee(EmployeeRole.LineManager);
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var notificationService = new NotificationService(dbContext);
        var service = new LeaveService(dbContext, notificationService);

        var submitted = await service.SubmitAsync(employee.Id, new SubmitLeaveRequest(
            LeaveType.Annual, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 3), "Trip", "0820000000"));
        await service.LineManagerReviewAsync(
            submitted.Value!.Id, manager.Id, new ReviewLeaveRequest(true, null, SamplePngBase64));
        var reviewed = await service.HRReviewAsync(
            submitted.Value.Id, hr.Id, new ReviewLeaveRequest(true, null, SamplePngBase64));

        Assert.True(reviewed.Succeeded);
        Assert.Equal(LeaveRequestStatus.Approved, reviewed.Value!.Status);

        var signedDocument = await service.GenerateSignedDocumentAsync(submitted.Value.Id, employee.Id);
        Assert.True(signedDocument.Succeeded);
        Assert.NotEmpty(signedDocument.Value!);
    }
}
