using GoldFieldsHR.Application.Incidents;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Incidents;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Incidents;

public class IncidentServiceTests
{
    private static async Task<Guid> SubmitIncidentAsync(IncidentService service, Guid employeeId)
    {
        var result = await service.SubmitAsync(employeeId, new SubmitIncidentReportRequest(
            "Loose rock", "Loose rock spotted near shaft entrance", IncidentSeverity.Medium,
            "Shaft 3", DateTime.UtcNow));
        return result.Value!.Id;
    }

    [Fact]
    public async Task UpdateStatus_ForwardTransition_Succeeds()
    {
        using var dbContext = TestDbContextFactory.Create();
        var reporter = dbContext.AddEmployee(EmployeeRole.Employee);
        var safetyOfficer = dbContext.AddEmployee(EmployeeRole.SafetyOfficer);
        var service = new IncidentService(dbContext);
        var incidentId = await SubmitIncidentAsync(service, reporter.Id);

        var result = await service.UpdateStatusAsync(
            incidentId, safetyOfficer.Id, new UpdateIncidentStatusRequest(IncidentStatus.UnderInvestigation, null));

        Assert.True(result.Succeeded);
        Assert.Equal(IncidentStatus.UnderInvestigation, result.Value!.Status);
    }

    [Fact]
    public async Task UpdateStatus_SkipAheadFromReportedToClosed_Succeeds()
    {
        // Business rule is "must move strictly forward", not "must pass through every stage".
        using var dbContext = TestDbContextFactory.Create();
        var reporter = dbContext.AddEmployee(EmployeeRole.Employee);
        var safetyOfficer = dbContext.AddEmployee(EmployeeRole.SafetyOfficer);
        var service = new IncidentService(dbContext);
        var incidentId = await SubmitIncidentAsync(service, reporter.Id);

        var result = await service.UpdateStatusAsync(
            incidentId, safetyOfficer.Id, new UpdateIncidentStatusRequest(IncidentStatus.Closed, "Resolved immediately"));

        Assert.True(result.Succeeded);
        Assert.Equal(IncidentStatus.Closed, result.Value!.Status);
    }

    [Fact]
    public async Task UpdateStatus_BackwardTransition_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var reporter = dbContext.AddEmployee(EmployeeRole.Employee);
        var safetyOfficer = dbContext.AddEmployee(EmployeeRole.SafetyOfficer);
        var service = new IncidentService(dbContext);
        var incidentId = await SubmitIncidentAsync(service, reporter.Id);

        await service.UpdateStatusAsync(
            incidentId, safetyOfficer.Id, new UpdateIncidentStatusRequest(IncidentStatus.UnderInvestigation, null));
        var result = await service.UpdateStatusAsync(
            incidentId, safetyOfficer.Id, new UpdateIncidentStatusRequest(IncidentStatus.Reported, null));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task UpdateStatus_SameStatusAgain_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var reporter = dbContext.AddEmployee(EmployeeRole.Employee);
        var safetyOfficer = dbContext.AddEmployee(EmployeeRole.SafetyOfficer);
        var service = new IncidentService(dbContext);
        var incidentId = await SubmitIncidentAsync(service, reporter.Id);

        await service.UpdateStatusAsync(
            incidentId, safetyOfficer.Id, new UpdateIncidentStatusRequest(IncidentStatus.Closed, null));
        var result = await service.UpdateStatusAsync(
            incidentId, safetyOfficer.Id, new UpdateIncidentStatusRequest(IncidentStatus.Closed, null));

        Assert.False(result.Succeeded);
    }
}
