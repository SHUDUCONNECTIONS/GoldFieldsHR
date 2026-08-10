using GoldFieldsHR.Application.Account;
using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Infrastructure.Identity;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Account;

public class AccountService(UserManager<AppUser> userManager, ApplicationDbContext dbContext) : IAccountService
{
    public async Task<ProfileDto?> GetProfileAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees
            .Include(e => e.Site)
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);

        if (employee is null)
        {
            return null;
        }

        var user = await userManager.FindByIdAsync(employee.UserId.ToString());

        return new ProfileDto(
            employee.Id,
            employee.FullName,
            user?.Email ?? string.Empty,
            employee.EmployeeNumber,
            employee.JobTitle,
            employee.Role,
            employee.Site?.Name ?? string.Empty);
    }

    public async Task<Result<bool>> ChangePasswordAsync(
        Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result<bool>.Failure("User not found.");
        }

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        return result.Succeeded
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(string.Join(" ", result.Errors.Select(e => e.Description)));
    }
}
