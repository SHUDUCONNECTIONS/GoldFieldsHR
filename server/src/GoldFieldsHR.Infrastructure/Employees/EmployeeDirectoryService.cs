using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Employees;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Identity;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Employees;

public class EmployeeDirectoryService(ApplicationDbContext dbContext, UserManager<AppUser> userManager) : IEmployeeDirectoryService
{
    public async Task<PagedResult<EmployeeSummaryDto>> GetPagedAsync(EmployeeDirectoryQuery query, CancellationToken cancellationToken = default)
    {
        var employeesQuery = dbContext.Employees
            .Include(e => e.Site)
            .Include(e => e.Manager)
            .AsQueryable();

        if (query.Role.HasValue)
        {
            employeesQuery = employeesQuery.Where(e => e.Role == query.Role.Value);
        }

        if (query.IsActive.HasValue)
        {
            employeesQuery = employeesQuery.Where(e => e.IsActive == query.IsActive.Value);
        }

        if (query.HasRequestedRole.HasValue)
        {
            employeesQuery = query.HasRequestedRole.Value
                ? employeesQuery.Where(e => e.RequestedRole != null)
                : employeesQuery.Where(e => e.RequestedRole == null);
        }

        var joined =
            from e in employeesQuery
            join u in dbContext.Users on e.UserId equals u.Id into userGroup
            from u in userGroup.DefaultIfEmpty()
            select new { Employee = e, Email = u != null ? (u.Email ?? string.Empty) : string.Empty };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            joined = joined.Where(x =>
                x.Employee.FirstName.ToLower().Contains(term) ||
                x.Employee.LastName.ToLower().Contains(term) ||
                x.Employee.EmployeeNumber.ToLower().Contains(term) ||
                x.Employee.JobTitle.ToLower().Contains(term) ||
                x.Email.ToLower().Contains(term));
        }

        var totalCount = await joined.CountAsync(cancellationToken);

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 1000);

        var pageRows = await joined
            .OrderBy(x => x.Employee.FirstName)
            .ThenBy(x => x.Employee.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = pageRows.Select(x => ToDto(x.Employee, x.Email)).ToList();

        return new PagedResult<EmployeeSummaryDto>(items, totalCount, page, pageSize);
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

        var actingEmployee = await dbContext.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == actingEmployeeId, cancellationToken);

        if (actingEmployee is null)
        {
            return Result<EmployeeSummaryDto>.Failure("Acting employee not found.");
        }

        var employee = await dbContext.Employees
            .Include(e => e.Site)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);

        if (employee is null)
        {
            return Result<EmployeeSummaryDto>.Failure("Employee not found.");
        }

        // Executives may only approve an employee's own pending role request, not
        // reassign roles arbitrarily — that stays an HR-only power.
        if (actingEmployee.Role == EmployeeRole.Executive && request.Role != employee.RequestedRole)
        {
            return Result<EmployeeSummaryDto>.Failure("Executives may only approve an employee's pending requested role.");
        }

        if (employee.Role == request.Role)
        {
            employee.RequestedRole = null;
            await dbContext.SaveChangesAsync(cancellationToken);
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
        employee.RequestedRole = null;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<EmployeeSummaryDto>.Success(ToDto(employee, user.Email ?? string.Empty));
    }

    public async Task<Result<EmployeeSummaryDto>> DismissRequestedRoleAsync(
        Guid employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees
            .Include(e => e.Site)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);

        if (employee is null)
        {
            return Result<EmployeeSummaryDto>.Failure("Employee not found.");
        }

        employee.RequestedRole = null;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<EmployeeSummaryDto>.Success(ToDto(employee, await GetEmailAsync(employee.UserId, cancellationToken)));
    }

    public async Task<IReadOnlyList<EmployeeSummaryDto>> GetDirectReportsAsync(
        Guid managerId, CancellationToken cancellationToken = default)
    {
        var joined =
            from e in dbContext.Employees
                .Include(e => e.Site)
                .Include(e => e.Manager)
                .Where(e => e.ManagerId == managerId)
            join u in dbContext.Users on e.UserId equals u.Id into userGroup
            from u in userGroup.DefaultIfEmpty()
            select new { Employee = e, Email = u != null ? (u.Email ?? string.Empty) : string.Empty };

        var rows = await joined
            .OrderBy(x => x.Employee.FirstName)
            .ThenBy(x => x.Employee.LastName)
            .ToListAsync(cancellationToken);

        return rows.Select(x => ToDto(x.Employee, x.Email)).ToList();
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

    public async Task<IReadOnlyList<EmployeeLiteDto>> GetActiveDirectoryLiteAsync(CancellationToken cancellationToken = default)
    {
        var employees = await dbContext.Employees
            .Include(e => e.Site)
            .Where(e => e.IsActive)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync(cancellationToken);

        return employees
            .Select(e => new EmployeeLiteDto(e.Id, e.FullName, e.JobTitle, e.Site?.Name ?? string.Empty))
            .ToList();
    }

    private static EmployeeSummaryDto ToDto(Domain.Entities.Employee employee, string email) => new(
        employee.Id,
        employee.EmployeeNumber,
        employee.FullName,
        email,
        employee.JobTitle,
        employee.Role,
        employee.RequestedRole,
        employee.Site?.Name ?? string.Empty,
        employee.IsActive,
        employee.CreatedAtUtc,
        employee.ManagerId,
        employee.Manager?.FullName);
}
