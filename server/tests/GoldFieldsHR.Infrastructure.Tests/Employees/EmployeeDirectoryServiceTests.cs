using GoldFieldsHR.Application.Employees;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Employees;
using GoldFieldsHR.Infrastructure.Identity;
using Moq;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Employees;

public class EmployeeDirectoryServiceTests
{
    [Fact]
    public async Task SetActiveStatus_OwnAccount_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var service = new EmployeeDirectoryService(dbContext, MockUserManagerFactory.Create().Object);

        var result = await service.SetActiveStatusAsync(hr.Id, hr.Id, new SetEmployeeActiveStatusRequest(false));

        Assert.False(result.Succeeded);
        Assert.Equal("You cannot change your own active status.", result.Error);
    }

    [Fact]
    public async Task SetActiveStatus_OtherEmployee_UpdatesIsActive()
    {
        using var dbContext = TestDbContextFactory.Create();
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = new EmployeeDirectoryService(dbContext, MockUserManagerFactory.Create().Object);

        var result = await service.SetActiveStatusAsync(employee.Id, hr.Id, new SetEmployeeActiveStatusRequest(false));

        Assert.True(result.Succeeded);
        Assert.False(result.Value!.IsActive);
    }

    [Fact]
    public async Task SetManager_ValidEmployeeNumber_AssignsManager()
    {
        using var dbContext = TestDbContextFactory.Create();
        var manager = dbContext.AddEmployee(EmployeeRole.LineManager, "LM-1");
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = new EmployeeDirectoryService(dbContext, MockUserManagerFactory.Create().Object);

        var result = await service.SetManagerAsync(employee.Id, new SetEmployeeManagerRequest("LM-1"));

        Assert.True(result.Succeeded);
        Assert.Equal(manager.Id, result.Value!.ManagerId);
        Assert.Equal(manager.FullName, result.Value.ManagerName);
    }

    [Fact]
    public async Task SetManager_UnknownEmployeeNumber_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = new EmployeeDirectoryService(dbContext, MockUserManagerFactory.Create().Object);

        var result = await service.SetManagerAsync(employee.Id, new SetEmployeeManagerRequest("DOES-NOT-EXIST"));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task SetManager_SelfAsManager_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee, "EMP-1");
        var service = new EmployeeDirectoryService(dbContext, MockUserManagerFactory.Create().Object);

        var result = await service.SetManagerAsync(employee.Id, new SetEmployeeManagerRequest("EMP-1"));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task SetManager_EmptyValue_ClearsManager()
    {
        using var dbContext = TestDbContextFactory.Create();
        var manager = dbContext.AddEmployee(EmployeeRole.LineManager, "LM-1");
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        employee.ManagerId = manager.Id;
        dbContext.SaveChanges();
        var service = new EmployeeDirectoryService(dbContext, MockUserManagerFactory.Create().Object);

        var result = await service.SetManagerAsync(employee.Id, new SetEmployeeManagerRequest(null));

        Assert.True(result.Succeeded);
        Assert.Null(result.Value!.ManagerId);
    }

    [Fact]
    public async Task GetAll_ReturnsEveryEmployee()
    {
        using var dbContext = TestDbContextFactory.Create();
        dbContext.AddEmployee(EmployeeRole.HR);
        dbContext.AddEmployee(EmployeeRole.Employee);
        dbContext.AddEmployee(EmployeeRole.LineManager);
        var service = new EmployeeDirectoryService(dbContext, MockUserManagerFactory.Create().Object);

        var result = await service.GetAllAsync();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task SetRole_OwnAccount_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var service = new EmployeeDirectoryService(dbContext, MockUserManagerFactory.Create().Object);

        var result = await service.SetRoleAsync(hr.Id, hr.Id, new SetEmployeeRoleRequest(EmployeeRole.Executive));

        Assert.False(result.Succeeded);
        Assert.Equal("You cannot change your own role.", result.Error);
    }

    [Fact]
    public async Task SetRole_SameRole_SucceedsAsNoOp()
    {
        using var dbContext = TestDbContextFactory.Create();
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = new EmployeeDirectoryService(dbContext, MockUserManagerFactory.Create().Object);

        var result = await service.SetRoleAsync(employee.Id, hr.Id, new SetEmployeeRoleRequest(EmployeeRole.Employee));

        Assert.True(result.Succeeded);
        Assert.Equal(EmployeeRole.Employee, result.Value!.Role);
    }

    [Fact]
    public async Task SetRole_NoLinkedAccount_Fails()
    {
        using var dbContext = TestDbContextFactory.Create();
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var service = new EmployeeDirectoryService(dbContext, MockUserManagerFactory.Create().Object);

        var result = await service.SetRoleAsync(employee.Id, hr.Id, new SetEmployeeRoleRequest(EmployeeRole.SafetyOfficer));

        Assert.False(result.Succeeded);
        Assert.Equal("No account is linked to this employee.", result.Error);
    }

    [Fact]
    public async Task SetRole_ValidChange_SyncsIdentityRolesAndUpdatesEmployee()
    {
        using var dbContext = TestDbContextFactory.Create();
        var hr = dbContext.AddEmployee(EmployeeRole.HR);
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var appUser = new AppUser { Id = employee.UserId, Email = "promoted@example.com" };

        var userManagerMock = MockUserManagerFactory.Create();
        userManagerMock.Setup(m => m.FindByIdAsync(employee.UserId.ToString())).ReturnsAsync(appUser);
        var service = new EmployeeDirectoryService(dbContext, userManagerMock.Object);

        var result = await service.SetRoleAsync(employee.Id, hr.Id, new SetEmployeeRoleRequest(EmployeeRole.SafetyOfficer));

        Assert.True(result.Succeeded);
        Assert.Equal(EmployeeRole.SafetyOfficer, result.Value!.Role);
        userManagerMock.Verify(m => m.RemoveFromRoleAsync(appUser, "Employee"), Times.Once);
        userManagerMock.Verify(m => m.AddToRoleAsync(appUser, "SafetyOfficer"), Times.Once);
    }
}
