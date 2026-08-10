using GoldFieldsHR.Application.Medical;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Medical;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Medical;

public class MedicalServiceTests
{
    [Fact]
    public async Task Record_FitWithRestrictions_WithoutRestrictionsText_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var nurse = dbContext.AddEmployee(EmployeeRole.Medical);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee, "EMP-1");
        var service = new MedicalService(dbContext);

        var result = await service.RecordAsync(nurse.Id, new RecordMedicalExaminationRequest(
            "EMP-1", new DateOnly(2026, 8, 1), new DateOnly(2027, 8, 1), FitnessStatus.FitWithRestrictions, null, null));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Record_FitWithRestrictions_WithRestrictionsText_Succeeds()
    {
        using var dbContext = TestDbContextFactory.Create();
        var nurse = dbContext.AddEmployee(EmployeeRole.Medical);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee, "EMP-1");
        var service = new MedicalService(dbContext);

        var result = await service.RecordAsync(nurse.Id, new RecordMedicalExaminationRequest(
            "EMP-1", new DateOnly(2026, 8, 1), new DateOnly(2027, 8, 1), FitnessStatus.FitWithRestrictions,
            "No heavy lifting", null));

        Assert.True(result.Succeeded);
        Assert.Equal("No heavy lifting", result.Value!.Restrictions);
    }

    [Fact]
    public async Task Record_ExpiryBeforeExamDate_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var nurse = dbContext.AddEmployee(EmployeeRole.Medical);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee, "EMP-1");
        var service = new MedicalService(dbContext);

        var result = await service.RecordAsync(nurse.Id, new RecordMedicalExaminationRequest(
            "EMP-1", new DateOnly(2026, 8, 1), new DateOnly(2026, 7, 1), FitnessStatus.Fit, null, null));

        Assert.False(result.Succeeded);
    }
}
