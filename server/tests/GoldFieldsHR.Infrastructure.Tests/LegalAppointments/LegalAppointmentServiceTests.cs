using GoldFieldsHR.Application.LegalAppointments;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.LegalAppointments;
using GoldFieldsHR.Infrastructure.Notifications;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.LegalAppointments;

public class LegalAppointmentServiceTests
{
    [Fact]
    public async Task Submit_ValidToBeforeValidFrom_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = new LegalAppointmentService(dbContext, new NotificationService(dbContext));

        var result = await service.SubmitAsync(employee.Id, new SubmitLegalAppointmentRequest(
            LegalAppointmentType.MineManager2_1, "J. Smith, CEO", "Overall control of the mine",
            new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 5)));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task FullWorkflow_SubmitApproveRevoke_MovesThroughStatusesCorrectly()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var safetyOfficer = dbContext.AddEmployee(EmployeeRole.SafetyOfficer);
        var service = new LegalAppointmentService(dbContext, new NotificationService(dbContext));

        var submitted = await service.SubmitAsync(employee.Id, new SubmitLegalAppointmentRequest(
            LegalAppointmentType.Engineer2_6_1, "J. Smith, Mine Manager", "Engineering oversight",
            new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 11)));
        Assert.Equal(LegalAppointmentStatus.Pending, submitted.Value!.Status);

        var approved = await service.ReviewAsync(submitted.Value.Id, safetyOfficer.Id, new ReviewLegalAppointmentRequest(true, null));
        Assert.Equal(LegalAppointmentStatus.Active, approved.Value!.Status);

        var active = await service.GetActiveAppointmentsAsync();
        Assert.Single(active);

        var revoked = await service.RevokeAsync(submitted.Value.Id, new RevokeLegalAppointmentRequest("No longer required"));
        Assert.True(revoked.Succeeded);
        Assert.Equal(LegalAppointmentStatus.Revoked, revoked.Value!.Status);
        Assert.NotNull(revoked.Value.RevokedAtUtc);

        Assert.Empty(await service.GetActiveAppointmentsAsync());
    }

    [Fact]
    public async Task Revoke_RejectedAppointment_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var safetyOfficer = dbContext.AddEmployee(EmployeeRole.SafetyOfficer);
        var service = new LegalAppointmentService(dbContext, new NotificationService(dbContext));

        var submitted = await service.SubmitAsync(employee.Id, new SubmitLegalAppointmentRequest(
            LegalAppointmentType.CompetentPerson3_1a, "J. Smith, Mine Manager", "Competent person for excavation",
            new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 11)));
        await service.ReviewAsync(submitted.Value!.Id, safetyOfficer.Id, new ReviewLegalAppointmentRequest(false, "Missing competency assessment"));

        var result = await service.RevokeAsync(submitted.Value.Id, new RevokeLegalAppointmentRequest(null));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Revoke_AlreadyRevoked_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var safetyOfficer = dbContext.AddEmployee(EmployeeRole.SafetyOfficer);
        var service = new LegalAppointmentService(dbContext, new NotificationService(dbContext));

        var submitted = await service.SubmitAsync(employee.Id, new SubmitLegalAppointmentRequest(
            LegalAppointmentType.ShiftSupervisor12_1, "J. Smith, Mine Manager", "Shift supervision",
            new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 11)));
        await service.ReviewAsync(submitted.Value!.Id, safetyOfficer.Id, new ReviewLegalAppointmentRequest(true, null));
        await service.RevokeAsync(submitted.Value.Id, new RevokeLegalAppointmentRequest(null));

        var result = await service.RevokeAsync(submitted.Value.Id, new RevokeLegalAppointmentRequest(null));

        Assert.False(result.Succeeded);
    }
}
