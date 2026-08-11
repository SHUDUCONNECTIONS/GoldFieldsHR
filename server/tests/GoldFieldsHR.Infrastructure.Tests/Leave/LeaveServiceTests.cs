using GoldFieldsHR.Application.Leave;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Leave;
using GoldFieldsHR.Infrastructure.Notifications;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Leave;

public class LeaveServiceTests
{
    [Fact]
    public async Task GetPendingApprovals_DirectReportsSurfaceFirst_ButOthersStillIncluded()
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

        var pending = await service.GetPendingApprovalsAsync(manager.Id);

        Assert.Equal(2, pending.Count);
        Assert.True(pending[0].IsDirectReport);
        Assert.Equal(directReport.Id, pending[0].EmployeeId);
        Assert.False(pending[1].IsDirectReport);
    }

    [Fact]
    public async Task GetPendingApprovals_NoDirectReports_StillReturnsSiteWideFallback()
    {
        using var dbContext = TestDbContextFactory.Create();
        var manager = dbContext.AddEmployee(EmployeeRole.LineManager);
        var unrelatedEmployee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = new LeaveService(dbContext, new NotificationService(dbContext));

        await service.SubmitAsync(unrelatedEmployee.Id, new SubmitLeaveRequest(
            LeaveType.Annual, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 3), "Trip", "0820000000"));

        var pending = await service.GetPendingApprovalsAsync(manager.Id);

        Assert.Single(pending);
        Assert.False(pending[0].IsDirectReport);
    }

    [Fact]
    public async Task ReviewAsync_NotifiesTheRequester()
    {
        using var dbContext = TestDbContextFactory.Create();
        var manager = dbContext.AddEmployee(EmployeeRole.LineManager);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var notificationService = new NotificationService(dbContext);
        var service = new LeaveService(dbContext, notificationService);

        var submitted = await service.SubmitAsync(employee.Id, new SubmitLeaveRequest(
            LeaveType.Annual, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 3), "Trip", "0820000000"));
        await service.ReviewAsync(submitted.Value!.Id, manager.Id, new ReviewLeaveRequest(true, null));

        var notifications = await notificationService.GetMineAsync(employee.Id);

        Assert.Single(notifications);
        Assert.Contains("approved", notifications[0].Message);
    }
}
