using GoldFieldsHR.Application.Certificates;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Certificates;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Certificates;

public class CertificateServiceTests
{
    [Fact]
    public async Task Issue_ExpiryFarInFuture_IsValid()
    {
        using var dbContext = TestDbContextFactory.Create();
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee, "EMP-1");
        var service = new CertificateService(dbContext);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await service.IssueAsync(hr.Id, new IssueCertificateRequest(
            "EMP-1", "First Aid", today, today.AddDays(200), null));

        Assert.True(result.Succeeded);
        Assert.Equal(CertificateStatus.Valid, result.Value!.Status);
    }

    [Fact]
    public async Task Issue_ExpiryWithinThirtyDays_IsDueSoon()
    {
        using var dbContext = TestDbContextFactory.Create();
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee, "EMP-1");
        var service = new CertificateService(dbContext);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await service.IssueAsync(hr.Id, new IssueCertificateRequest(
            "EMP-1", "Forklift License", today.AddDays(-300), today.AddDays(10), null));

        Assert.True(result.Succeeded);
        Assert.Equal(CertificateStatus.DueSoon, result.Value!.Status);
    }

    [Fact]
    public async Task Issue_ExpiryInPast_IsExpired()
    {
        using var dbContext = TestDbContextFactory.Create();
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee, "EMP-1");
        var service = new CertificateService(dbContext);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await service.IssueAsync(hr.Id, new IssueCertificateRequest(
            "EMP-1", "Confined Space Entry", today.AddDays(-400), today.AddDays(-10), null));

        Assert.True(result.Succeeded);
        Assert.Equal(CertificateStatus.Expired, result.Value!.Status);
    }

    [Fact]
    public async Task Issue_ExpiryBeforeIssuedDate_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee, "EMP-1");
        var service = new CertificateService(dbContext);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await service.IssueAsync(hr.Id, new IssueCertificateRequest(
            "EMP-1", "First Aid", today, today.AddDays(-5), null));

        Assert.False(result.Succeeded);
    }
}
