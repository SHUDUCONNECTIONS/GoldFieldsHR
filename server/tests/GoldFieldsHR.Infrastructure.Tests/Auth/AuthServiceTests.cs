using GoldFieldsHR.Application.Auth;
using GoldFieldsHR.Application.Notifications;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Auth;
using GoldFieldsHR.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace GoldFieldsHR.Infrastructure.Tests.Auth;

public class AuthServiceTests
{
    private static JwtTokenGenerator CreateTokenGenerator() => new(Options.Create(new JwtSettings
    {
        Key = "test-signing-key-at-least-32-characters-long!",
        Issuer = "Test",
        Audience = "Test",
        ExpiryMinutes = 60,
    }));

    private static INotificationService CreateNotificationService() => Mock.Of<INotificationService>();

    [Fact]
    public async Task Login_LockedOutAccount_FailsWithoutCheckingPassword()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var user = new AppUser { Id = employee.UserId, Email = "locked@example.com", UserName = "locked@example.com" };

        var userManager = MockUserManagerFactory.Create();
        userManager.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        userManager.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(true);

        var service = new AuthService(userManager.Object, dbContext, CreateTokenGenerator(), CreateNotificationService());

        var result = await service.LoginAsync(new LoginRequest(user.Email, "whatever"));

        Assert.False(result.Succeeded);
        Assert.Contains("locked", result.Errors.Single(), StringComparison.OrdinalIgnoreCase);
        userManager.Verify(m => m.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Login_WrongPassword_RecordsFailedAttempt()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var user = new AppUser { Id = employee.UserId, Email = "wrongpw@example.com", UserName = "wrongpw@example.com" };

        var userManager = MockUserManagerFactory.Create();
        userManager.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        userManager.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(false);
        userManager.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(false);
        userManager.Setup(m => m.AccessFailedAsync(user)).ReturnsAsync(IdentityResult.Success);

        var service = new AuthService(userManager.Object, dbContext, CreateTokenGenerator(), CreateNotificationService());

        var result = await service.LoginAsync(new LoginRequest(user.Email, "wrong-password"));

        Assert.False(result.Succeeded);
        userManager.Verify(m => m.AccessFailedAsync(user), Times.Once);
    }

    [Fact]
    public async Task Login_CorrectPassword_ResetsFailedAttemptCounter()
    {
        using var dbContext = TestDbContextFactory.Create();
        var employee = dbContext.AddEmployee(EmployeeRole.Employee);
        var user = new AppUser { Id = employee.UserId, Email = "correct@example.com", UserName = "correct@example.com" };

        var userManager = MockUserManagerFactory.Create();
        userManager.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        userManager.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(false);
        userManager.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(true);
        userManager.Setup(m => m.ResetAccessFailedCountAsync(user)).ReturnsAsync(IdentityResult.Success);

        var service = new AuthService(userManager.Object, dbContext, CreateTokenGenerator(), CreateNotificationService());

        var result = await service.LoginAsync(new LoginRequest(user.Email, "correct-password"));

        Assert.True(result.Succeeded);
        userManager.Verify(m => m.ResetAccessFailedCountAsync(user), Times.Once);
    }

    [Fact]
    public async Task Login_UnknownEmail_FailsWithoutLockoutCheck()
    {
        using var dbContext = TestDbContextFactory.Create();
        var userManager = MockUserManagerFactory.Create();
        userManager.Setup(m => m.FindByEmailAsync("nobody@example.com")).ReturnsAsync((AppUser?)null);

        var service = new AuthService(userManager.Object, dbContext, CreateTokenGenerator(), CreateNotificationService());

        var result = await service.LoginAsync(new LoginRequest("nobody@example.com", "whatever"));

        Assert.False(result.Succeeded);
        userManager.Verify(m => m.IsLockedOutAsync(It.IsAny<AppUser>()), Times.Never);
    }
}
