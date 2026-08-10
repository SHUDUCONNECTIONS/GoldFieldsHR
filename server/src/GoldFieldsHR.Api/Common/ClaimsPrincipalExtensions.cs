using System.Security.Claims;

namespace GoldFieldsHR.Api.Common;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetEmployeeId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("employeeId");
        return Guid.TryParse(value, out var employeeId)
            ? employeeId
            : throw new InvalidOperationException("The current user does not have an employeeId claim.");
    }

    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new InvalidOperationException("The current user does not have a valid subject claim.");
    }
}
