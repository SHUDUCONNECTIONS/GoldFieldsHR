using GoldFieldsHR.Application.Kpi;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Kpi;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Kpi;

public class KpiServiceTests
{
    private const string TestSignaturePngBase64 =
        "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=";

    private static CreateKpiTemplateRequest SampleTemplateRequest() => new(
        "Engineering Foreman",
        [
            new CreateKpiTemplateCategoryRequest("SAFETY & HEALTH", [
                new CreateKpiTemplateItemRequest("Attend daily internal safety meeting (INVOCOM)", null),
                new CreateKpiTemplateItemRequest("Submit VFL", null),
            ]),
            new CreateKpiTemplateCategoryRequest("QUALITY & QUANTITY BLAST", [
                new CreateKpiTemplateItemRequest("Work Attendance (timesheet)", "Daily"),
                new CreateKpiTemplateItemRequest("Conduct emergency drills", "Weekly"),
            ]),
        ]);

    private static async Task<(KpiTemplate Template, Employee Employee, Employee Manager, Employee Officer, Employee Engineer, Employee Hr, KpiService Service, Persistence.ApplicationDbContext DbContext)>
        SeedAppraisalFixtureAsync()
    {
        var dbContext = TestDbContextFactory.Create();
        var service = new KpiService(dbContext);

        var hr = dbContext.AddEmployee(EmployeeRole.HR, "HR-1");
        var manager = dbContext.AddEmployee(EmployeeRole.LineManager, "LM-1");
        var employee = dbContext.AddEmployee(EmployeeRole.Employee, "EMP-1");
        employee.ManagerId = manager.Id;
        var officer = dbContext.AddEmployee(EmployeeRole.Employee, "OFFICER-1");
        var engineer = dbContext.AddEmployee(EmployeeRole.Employee, "ENGINEER-1");
        await dbContext.SaveChangesAsync();

        var templateResult = await service.CreateTemplateAsync(SampleTemplateRequest());
        Assert.True(templateResult.Succeeded);

        var template = await dbContext.KpiTemplates.FindAsync(templateResult.Value!.Id);

        return (template!, employee, manager, officer, engineer, hr, service, dbContext);
    }

    [Fact]
    public async Task CreateTemplate_SnapshotsCategoriesAndItemsInOrder()
    {
        var dbContext = TestDbContextFactory.Create();
        var service = new KpiService(dbContext);

        var result = await service.CreateTemplateAsync(SampleTemplateRequest());

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value!.Categories.Count);
        Assert.Equal("SAFETY & HEALTH", result.Value.Categories[0].Name);
        Assert.Equal(2, result.Value.Categories[0].Items.Count);
        Assert.Equal("Daily", result.Value.Categories[1].Items[0].SubGroupLabel);
    }

    [Fact]
    public async Task CreateAppraisal_SnapshotsAllTemplateItems()
    {
        var (template, employee, _, officer, engineer, _, service, _) = await SeedAppraisalFixtureAsync();

        var result = await service.CreateAppraisalAsync(
            employee.Id,
            new CreateKpiAppraisalRequest("EMP-1", template.Id, "2026", null, "OFFICER-1", "ENGINEER-1", null, null, null, null));

        Assert.True(result.Succeeded);
        Assert.Equal(4, result.Value!.Items.Count);
        Assert.Equal("InProgress", result.Value.Status);
    }

    [Fact]
    public async Task CreateAppraisal_UnknownEmployeeNumber_Fails()
    {
        var (template, employee, _, _, _, _, service, _) = await SeedAppraisalFixtureAsync();

        var result = await service.CreateAppraisalAsync(
            employee.Id,
            new CreateKpiAppraisalRequest("DOES-NOT-EXIST", template.Id, "2026", null, "OFFICER-1", "ENGINEER-1", null, null, null, null));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task SubmitCheckpointScores_ByEmployeesManager_UpdatesRollup()
    {
        var (template, employee, manager, _, _, _, service, _) = await SeedAppraisalFixtureAsync();

        var created = await service.CreateAppraisalAsync(
            employee.Id,
            new CreateKpiAppraisalRequest("EMP-1", template.Id, "2026", null, "OFFICER-1", "ENGINEER-1", null, null, null, null));
        var itemIds = created.Value!.Items.Select(i => i.Id).ToList();

        var scoreEntries = itemIds.Select(id => new KpiItemScoreEntry(id, 3, "Great")).ToList();
        var result = await service.SubmitCheckpointScoresAsync(
            created.Value.Id, manager.Id, new SubmitCheckpointScoresRequest(1, scoreEntries));

        Assert.True(result.Succeeded);
        Assert.Equal(100.0, result.Value!.OverallScorePercent);
    }

    [Fact]
    public async Task SubmitCheckpointScores_ByUnrelatedEmployee_Fails()
    {
        var (template, employee, _, _, _, _, service, dbContext) = await SeedAppraisalFixtureAsync();
        var stranger = dbContext.AddEmployee(EmployeeRole.Employee, "STRANGER-1");
        await dbContext.SaveChangesAsync();

        var created = await service.CreateAppraisalAsync(
            employee.Id,
            new CreateKpiAppraisalRequest("EMP-1", template.Id, "2026", null, "OFFICER-1", "ENGINEER-1", null, null, null, null));
        var itemId = created.Value!.Items[0].Id;

        var result = await service.SubmitCheckpointScoresAsync(
            created.Value.Id, stranger.Id, new SubmitCheckpointScoresRequest(1, [new KpiItemScoreEntry(itemId, 2, null)]));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task SignOff_FullTwoStageFlow_ReachesFinalized()
    {
        var (template, employee, _, officer, engineer, _, service, _) = await SeedAppraisalFixtureAsync();

        var created = await service.CreateAppraisalAsync(
            employee.Id,
            new CreateKpiAppraisalRequest("EMP-1", template.Id, "2026", null, "OFFICER-1", "ENGINEER-1", null, null, null, null));

        var officerSign = await service.SignAsBlastingOfficerAsync(
            created.Value!.Id, officer.Id, new SignKpiAppraisalRequest(TestSignaturePngBase64));
        Assert.True(officerSign.Succeeded);
        Assert.Equal("PendingBlastingEngineerSignOff", officerSign.Value!.Status);

        var engineerSign = await service.SignAsBlastingEngineerAsync(
            created.Value.Id, engineer.Id, new SignKpiAppraisalRequest(TestSignaturePngBase64));
        Assert.True(engineerSign.Succeeded);
        Assert.Equal("Finalized", engineerSign.Value!.Status);
        Assert.NotNull(engineerSign.Value.FinalizedAtUtc);
    }

    [Fact]
    public async Task SignAsBlastingOfficer_ByWrongEmployee_Fails()
    {
        var (template, employee, _, _, _, _, service, dbContext) = await SeedAppraisalFixtureAsync();
        var stranger = dbContext.AddEmployee(EmployeeRole.Employee, "STRANGER-2");
        await dbContext.SaveChangesAsync();

        var created = await service.CreateAppraisalAsync(
            employee.Id,
            new CreateKpiAppraisalRequest("EMP-1", template.Id, "2026", null, "OFFICER-1", "ENGINEER-1", null, null, null, null));

        var result = await service.SignAsBlastingOfficerAsync(
            created.Value!.Id, stranger.Id, new SignKpiAppraisalRequest(null));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task SignAsBlastingEngineer_BeforeOfficerSigns_Fails()
    {
        var (template, employee, _, _, engineer, _, service, _) = await SeedAppraisalFixtureAsync();

        var created = await service.CreateAppraisalAsync(
            employee.Id,
            new CreateKpiAppraisalRequest("EMP-1", template.Id, "2026", null, "OFFICER-1", "ENGINEER-1", null, null, null, null));

        var result = await service.SignAsBlastingEngineerAsync(
            created.Value!.Id, engineer.Id, new SignKpiAppraisalRequest(null));

        Assert.False(result.Succeeded);
    }
}
