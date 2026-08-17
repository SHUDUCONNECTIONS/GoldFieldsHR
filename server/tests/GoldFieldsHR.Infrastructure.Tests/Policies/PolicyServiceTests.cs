using GoldFieldsHR.Application.Policies;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Attachments;
using GoldFieldsHR.Infrastructure.Documents;
using GoldFieldsHR.Infrastructure.Notifications;
using GoldFieldsHR.Infrastructure.Persistence;
using GoldFieldsHR.Infrastructure.Policies;
using Microsoft.Extensions.Options;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Policies;

public class PolicyServiceTests
{
    // 1x1 transparent PNG, used wherever a test needs a stand-in signature image.
    private const string SamplePngBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=";

    private static PolicyService CreateService(ApplicationDbContext dbContext) => new(
        dbContext,
        new NotificationService(dbContext),
        new AttachmentService(dbContext, Options.Create(new FileStorageSettings())),
        new DocumentSigningService());

    [Fact]
    public async Task Acknowledge_FirstAcknowledgment_ReportsCountOfOne()
    {
        // Regression test: EF Core navigation fixup previously caused the freshly-added
        // acknowledgment to double-count itself in policy.Acknowledgments.Count.
        using var dbContext = TestDbContextFactory.Create();
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = CreateService(dbContext);

        var created = await service.CreateAsync(hr.Id, new CreatePolicyRequest("Site Safety Rules", "Wear PPE at all times."));
        Assert.True(created.Succeeded);

        var result = await service.AcknowledgeAsync(created.Value!.Id, employee.Id, new AcknowledgePolicyRequest(SamplePngBase64));

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
        var service = CreateService(dbContext);

        var created = await service.CreateAsync(hr.Id, new CreatePolicyRequest("Site Safety Rules", "Wear PPE at all times."));
        await service.AcknowledgeAsync(created.Value!.Id, employee1.Id, new AcknowledgePolicyRequest(SamplePngBase64));
        var result = await service.AcknowledgeAsync(created.Value.Id, employee2.Id, new AcknowledgePolicyRequest(SamplePngBase64));

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value!.AcknowledgmentCount);
    }

    [Fact]
    public async Task Acknowledge_AlreadyAcknowledged_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = CreateService(dbContext);

        var created = await service.CreateAsync(hr.Id, new CreatePolicyRequest("Site Safety Rules", "Wear PPE at all times."));
        await service.AcknowledgeAsync(created.Value!.Id, employee.Id, new AcknowledgePolicyRequest(SamplePngBase64));
        var result = await service.AcknowledgeAsync(created.Value.Id, employee.Id, new AcknowledgePolicyRequest(SamplePngBase64));

        Assert.False(result.Succeeded);
        Assert.Equal("You have already acknowledged this policy.", result.Error);
    }

    [Fact]
    public async Task Acknowledge_NoSignatureProvidedAndNoneOnFile_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = CreateService(dbContext);

        var created = await service.CreateAsync(hr.Id, new CreatePolicyRequest("Site Safety Rules", "Wear PPE at all times."));
        var result = await service.AcknowledgeAsync(created.Value!.Id, employee.Id, new AcknowledgePolicyRequest(null));

        Assert.False(result.Succeeded);
        Assert.Equal("Please sign to acknowledge this policy.", result.Error);
    }
}
