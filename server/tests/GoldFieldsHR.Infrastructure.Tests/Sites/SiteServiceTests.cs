using GoldFieldsHR.Application.Sites;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Sites;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Sites;

public class SiteServiceTests
{
    [Fact]
    public async Task Create_DuplicateName_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        dbContext.AddEmployee(EmployeeRole.HR); // ensures a site exists via the test factory
        var service = new SiteService(dbContext);
        var existingName = (await service.GetAllAsync())[0].Name;

        var result = await service.CreateAsync(new CreateSiteRequest(existingName, "Somewhere"));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Create_ValidSite_Succeeds()
    {
        using var dbContext = TestDbContextFactory.Create();
        var service = new SiteService(dbContext);

        var result = await service.CreateAsync(new CreateSiteRequest("New Mine Site", "Limpopo"));

        Assert.True(result.Succeeded);
        Assert.Equal("New Mine Site", result.Value!.Name);
        Assert.True(result.Value.IsActive);
        Assert.Equal(0, result.Value.EmployeeCount);
    }

    [Fact]
    public async Task SetActiveStatus_DeactivateSiteWithActiveEmployees_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = new SiteService(dbContext);

        var result = await service.SetActiveStatusAsync(employee.SiteId, new SetSiteActiveStatusRequest(false));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task SetActiveStatus_DeactivateEmptySite_Succeeds()
    {
        using var dbContext = TestDbContextFactory.Create();
        var service = new SiteService(dbContext);
        var created = await service.CreateAsync(new CreateSiteRequest("Empty Site", "Nowhere"));

        var result = await service.SetActiveStatusAsync(created.Value!.Id, new SetSiteActiveStatusRequest(false));

        Assert.True(result.Succeeded);
        Assert.False(result.Value!.IsActive);
    }

    [Fact]
    public async Task GetActive_ExcludesInactiveSites()
    {
        using var dbContext = TestDbContextFactory.Create();
        var service = new SiteService(dbContext);
        var created = await service.CreateAsync(new CreateSiteRequest("Temp Site", "Somewhere"));
        await service.SetActiveStatusAsync(created.Value!.Id, new SetSiteActiveStatusRequest(false));

        var active = await service.GetActiveAsync();

        Assert.DoesNotContain(active, s => s.Id == created.Value.Id);
    }

    [Fact]
    public async Task Update_DuplicateName_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var service = new SiteService(dbContext);
        await service.CreateAsync(new CreateSiteRequest("Site A", "Loc A"));
        var siteB = await service.CreateAsync(new CreateSiteRequest("Site B", "Loc B"));

        var result = await service.UpdateAsync(siteB.Value!.Id, new UpdateSiteRequest("Site A", "Loc B"));

        Assert.False(result.Succeeded);
    }
}
