using GoldFieldsHR.Application.Timesheet;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Notifications;
using GoldFieldsHR.Infrastructure.Persistence;
using GoldFieldsHR.Infrastructure.Timesheet;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Timesheet;

public class TimesheetServiceTests
{
    private static TimesheetService CreateService(ApplicationDbContext dbContext) =>
        new(dbContext, new NotificationService(dbContext));

    [Fact]
    public async Task ClockIn_ThenClockOut_ProducesCorrectDuration()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = CreateService(dbContext);

        var clockIn = await service.ClockInAsync(employee.Id);
        Assert.True(clockIn.Succeeded);
        Assert.Null(clockIn.Value!.ClockOutUtc);

        var clockOut = await service.ClockOutAsync(employee.Id);
        Assert.True(clockOut.Succeeded);
        Assert.NotNull(clockOut.Value!.DurationHours);
    }

    [Fact]
    public async Task SubmitCorrection_NoTimesProvided_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = CreateService(dbContext);
        var entry = (await service.ClockInAsync(employee.Id)).Value!;

        var result = await service.SubmitCorrectionAsync(
            employee.Id, new SubmitTimesheetCorrectionRequest(entry.Id, null, null, "Forgot to note actual time"));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task SubmitCorrection_ForSomeoneElsesEntry_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var otherEmployee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = CreateService(dbContext);
        var entry = (await service.ClockInAsync(employee.Id)).Value!;

        var result = await service.SubmitCorrectionAsync(
            otherEmployee.Id,
            new SubmitTimesheetCorrectionRequest(entry.Id, entry.ClockInUtc.AddHours(-1), null, "Not my entry"));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task SubmitCorrection_DuplicatePending_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = CreateService(dbContext);
        var entry = (await service.ClockInAsync(employee.Id)).Value!;

        await service.SubmitCorrectionAsync(
            employee.Id, new SubmitTimesheetCorrectionRequest(entry.Id, entry.ClockInUtc.AddHours(-1), null, "Started earlier"));
        var result = await service.SubmitCorrectionAsync(
            employee.Id, new SubmitTimesheetCorrectionRequest(entry.Id, entry.ClockInUtc.AddHours(-2), null, "Actually even earlier"));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ReviewCorrection_Approved_UpdatesTheUnderlyingEntry()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var manager = dbContext.AddEmployee(EmployeeRole.LineManager);
        var service = CreateService(dbContext);
        var entry = (await service.ClockInAsync(employee.Id)).Value!;
        await service.ClockOutAsync(employee.Id);

        var correctedClockIn = entry.ClockInUtc.AddHours(-1);
        var submitted = await service.SubmitCorrectionAsync(
            employee.Id, new SubmitTimesheetCorrectionRequest(entry.Id, correctedClockIn, null, "Forgot to clock in on time"));
        Assert.True(submitted.Succeeded);

        var reviewed = await service.ReviewCorrectionAsync(
            submitted.Value!.Id, manager.Id, new ReviewTimesheetCorrectionRequest(true, null));

        Assert.True(reviewed.Succeeded);
        Assert.Equal(TimesheetCorrectionStatus.Approved, reviewed.Value!.Status);

        var history = await service.GetHistoryAsync(employee.Id);
        Assert.Equal(correctedClockIn, history[0].ClockInUtc);
    }

    [Fact]
    public async Task ReviewCorrection_Rejected_LeavesEntryUnchanged()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var manager = dbContext.AddEmployee(EmployeeRole.LineManager);
        var service = CreateService(dbContext);
        var entry = (await service.ClockInAsync(employee.Id)).Value!;
        var originalClockIn = entry.ClockInUtc;

        var submitted = await service.SubmitCorrectionAsync(
            employee.Id, new SubmitTimesheetCorrectionRequest(entry.Id, entry.ClockInUtc.AddHours(-1), null, "Reason"));
        await service.ReviewCorrectionAsync(
            submitted.Value!.Id, manager.Id, new ReviewTimesheetCorrectionRequest(false, "Not credible"));

        var history = await service.GetHistoryAsync(employee.Id);
        Assert.Equal(originalClockIn, history[0].ClockInUtc);
    }

    [Fact]
    public async Task ReviewCorrection_AlreadyReviewed_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var manager = dbContext.AddEmployee(EmployeeRole.LineManager);
        var service = CreateService(dbContext);
        var entry = (await service.ClockInAsync(employee.Id)).Value!;

        var submitted = await service.SubmitCorrectionAsync(
            employee.Id, new SubmitTimesheetCorrectionRequest(entry.Id, entry.ClockInUtc.AddHours(-1), null, "Reason"));
        await service.ReviewCorrectionAsync(submitted.Value!.Id, manager.Id, new ReviewTimesheetCorrectionRequest(true, null));

        var result = await service.ReviewCorrectionAsync(submitted.Value.Id, manager.Id, new ReviewTimesheetCorrectionRequest(true, null));

        Assert.False(result.Succeeded);
    }
}
