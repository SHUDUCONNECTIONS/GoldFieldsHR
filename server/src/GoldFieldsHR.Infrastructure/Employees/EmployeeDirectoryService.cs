using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Employees;
using GoldFieldsHR.Infrastructure.Identity;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Employees;

public class EmployeeDirectoryService(ApplicationDbContext dbContext, UserManager<AppUser> userManager) : IEmployeeDirectoryService
{
    public async Task<IReadOnlyList<EmployeeSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var employees = await dbContext.Employees
            .Include(e => e.Site)
            .Include(e => e.Manager)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync(cancellationToken);

        var userIds = employees.Select(e => e.UserId).ToList();
        var emailsByUserId = await dbContext.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Email ?? string.Empty, cancellationToken);

        return employees.Select(e => ToDto(e, emailsByUserId.GetValueOrDefault(e.UserId, string.Empty))).ToList();
    }

    public async Task<Result<EmployeeSummaryDto>> SetActiveStatusAsync(
        Guid employeeId, Guid actingEmployeeId, SetEmployeeActiveStatusRequest request, CancellationToken cancellationToken = default)
    {
        if (employeeId == actingEmployeeId)
        {
            return Result<EmployeeSummaryDto>.Failure("You cannot change your own active status.");
        }

        var employee = await dbContext.Employees
            .Include(e => e.Site)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);

        if (employee is null)
        {
            return Result<EmployeeSummaryDto>.Failure("Employee not found.");
        }

        employee.IsActive = request.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);

        var email = await dbContext.Users
            .Where(u => u.Id == employee.UserId)
            .Select(u => u.Email ?? string.Empty)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        return Result<EmployeeSummaryDto>.Success(ToDto(employee, email));
    }

    public async Task<Result<EmployeeSummaryDto>> SetRoleAsync(
        Guid employeeId, Guid actingEmployeeId, SetEmployeeRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (employeeId == actingEmployeeId)
        {
            return Result<EmployeeSummaryDto>.Failure("You cannot change your own role.");
        }

        var employee = await dbContext.Employees
            .Include(e => e.Site)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);

        if (employee is null)
        {
            return Result<EmployeeSummaryDto>.Failure("Employee not found.");
        }

        if (employee.Role == request.Role)
        {
            return Result<EmployeeSummaryDto>.Success(ToDto(employee, await GetEmailAsync(employee.UserId, cancellationToken)));
        }

        var user = await userManager.FindByIdAsync(employee.UserId.ToString());
        if (user is null)
        {
            return Result<EmployeeSummaryDto>.Failure("No account is linked to this employee.");
        }

        var oldRoleName = employee.Role.ToString();
        var newRoleName = request.Role.ToString();

        await userManager.RemoveFromRoleAsync(user, oldRoleName);
        await userManager.AddToRoleAsync(user, newRoleName);

        employee.Role = request.Role;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<EmployeeSummaryDto>.Success(ToDto(employee, user.Email ?? string.Empty));
    }

    private async Task<string> GetEmailAsync(Guid userId, CancellationToken cancellationToken) =>
        await dbContext.Users.Where(u => u.Id == userId).Select(u => u.Email ?? string.Empty).FirstOrDefaultAsync(cancellationToken)
        ?? string.Empty;

    public async Task<Result<EmployeeSummaryDto>> SetManagerAsync(
        Guid employeeId, SetEmployeeManagerRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees
            .Include(e => e.Site)
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);

        if (employee is null)
        {
            return Result<EmployeeSummaryDto>.Failure("Employee not found.");
        }

        if (string.IsNullOrWhiteSpace(request.ManagerEmployeeNumber))
        {
            employee.ManagerId = null;
        }
        else
        {
            var manager = await dbContext.Employees
                .FirstOrDefaultAsync(e => e.EmployeeNumber == request.ManagerEmployeeNumber, cancellationToken);
            if (manager is null)
            {
                return Result<EmployeeSummaryDto>.Failure(
                    $"No employee found with number '{request.ManagerEmployeeNumber}'.");
            }

            if (manager.Id == employeeId)
            {
                return Result<EmployeeSummaryDto>.Failure("An employee cannot be their own manager.");
            }

            employee.ManagerId = manager.Id;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // Reload with the Manager navigation populated for an accurate response.
        await dbContext.Entry(employee).Reference(e => e.Manager).LoadAsync(cancellationToken);

        var email = await dbContext.Users
            .Where(u => u.Id == employee.UserId)
            .Select(u => u.Email ?? string.Empty)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        return Result<EmployeeSummaryDto>.Success(ToDto(employee, email));
    }

    private static EmployeeSummaryDto ToDto(Domain.Entities.Employee employee, string email) => new(
        employee.Id,
        employee.EmployeeNumber,
        employee.FullName,
        email,
        employee.JobTitle,
        employee.Role,
        employee.Site?.Name ?? string.Empty,
        employee.IsActive,
        employee.CreatedAtUtc,
        employee.ManagerId,
        employee.Manager?.FullName);
}
