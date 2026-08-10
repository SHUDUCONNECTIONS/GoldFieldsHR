using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Safety;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Safety;

public class SafetyService(ApplicationDbContext dbContext) : ISafetyService
{
    public async Task<Result<PreShiftSafetyCheckDto>> SubmitAsync(
        Guid employeeId, SubmitPreShiftCheckRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees.FindAsync([employeeId], cancellationToken);
        if (employee is null)
        {
            return Result<PreShiftSafetyCheckDto>.Failure("Employee profile not found.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var alreadySubmitted = await dbContext.PreShiftSafetyChecks
            .AnyAsync(c => c.EmployeeId == employeeId && c.CheckDate == today, cancellationToken);

        if (alreadySubmitted)
        {
            return Result<PreShiftSafetyCheckDto>.Failure("You have already completed today's pre-shift safety check.");
        }

        var entity = new PreShiftSafetyCheck
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            CheckDate = today,
            HazardsIdentified = request.HazardsIdentified,
            HazardNotes = request.HazardNotes,
        };

        dbContext.PreShiftSafetyChecks.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<PreShiftSafetyCheckDto>.Success(ToDto(entity, employee.FullName));
    }

    public async Task<PreShiftSafetyCheckDto?> GetTodayAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var entity = await dbContext.PreShiftSafetyChecks
            .Include(c => c.Employee)
            .FirstOrDefaultAsync(c => c.EmployeeId == employeeId && c.CheckDate == today, cancellationToken);

        return entity is null ? null : ToDto(entity, entity.Employee!.FullName);
    }

    public async Task<IReadOnlyList<PreShiftSafetyCheckDto>> GetHistoryAsync(
        Guid employeeId, CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.PreShiftSafetyChecks
            .Include(c => c.Employee)
            .Where(c => c.EmployeeId == employeeId)
            .OrderByDescending(c => c.CheckDate)
            .Take(30)
            .ToListAsync(cancellationToken);

        return entities.Select(c => ToDto(c, c.Employee!.FullName)).ToList();
    }

    public async Task<IReadOnlyList<PreShiftSafetyCheckDto>> GetTodaysHazardsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var entities = await dbContext.PreShiftSafetyChecks
            .Include(c => c.Employee)
            .Where(c => c.CheckDate == today && c.HazardsIdentified)
            .OrderByDescending(c => c.SubmittedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(c => ToDto(c, c.Employee!.FullName)).ToList();
    }

    private static PreShiftSafetyCheckDto ToDto(PreShiftSafetyCheck entity, string employeeName) => new(
        entity.Id,
        entity.EmployeeId,
        employeeName,
        entity.CheckDate,
        entity.HazardsIdentified,
        entity.HazardNotes,
        entity.SubmittedAtUtc);
}
