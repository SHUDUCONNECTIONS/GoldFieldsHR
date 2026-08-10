using GoldFieldsHR.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace GoldFieldsHR.Infrastructure.Tests;

public static class MockUserManagerFactory
{
    public static Mock<UserManager<AppUser>> Create()
    {
        var store = new Mock<IUserStore<AppUser>>();
        var manager = new Mock<UserManager<AppUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        manager.Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        manager.Setup(m => m.RemoveFromRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

        return manager;
    }
}
