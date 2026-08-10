using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Emergency;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Emergency;

public class EmergencyService(ApplicationDbContext dbContext) : IEmergencyService
{
    public async Task<Result<EmergencyAlertDto>> TriggerAsync(
        Guid employeeId, TriggerEmergencyAlertRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees.FindAsync([employeeId], cancellationToken);
        if (employee is null)
        {
            return Result<EmergencyAlertDto>.Failure("Employee profile not found.");
        }

        var hasActiveAlert = await dbContext.EmergencyAlerts
            .AnyAsync(a => a.EmployeeId == employeeId && a.Status == EmergencyAlertStatus.Active, cancellationToken);
        if (hasActiveAlert)
        {
            return Result<EmergencyAlertDto>.Failure("You already have an active SOS alert.");
        }

        var entity = new EmergencyAlert
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            Location = request.Location,
            Message = request.Message,
        };

        dbContext.EmergencyAlerts.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<EmergencyAlertDto>.Success(ToDto(entity, employee.FullName));
    }

    public async Task<IReadOnlyList<EmergencyAlertDto>> GetMyAlertsAsync(
        Guid employeeId, CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.EmergencyAlerts
            .Include(a => a.Employee)
            .Where(a => a.EmployeeId == employeeId)
            .OrderByDescending(a => a.TriggeredAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(a => ToDto(a, a.Employee!.FullName)).ToList();
    }

    public async Task<IReadOnlyList<EmergencyAlertDto>> GetActiveAlertsAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.EmergencyAlerts
            .Include(a => a.Employee)
            .Where(a => a.Status == EmergencyAlertStatus.Active)
            .OrderBy(a => a.TriggeredAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(a => ToDto(a, a.Employee!.FullName)).ToList();
    }

    public async Task<Result<EmergencyAlertDto>> ResolveAsync(
        Guid alertId, Guid resolverId, ResolveEmergencyAlertRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.EmergencyAlerts
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == alertId, cancellationToken);

        if (entity is null)
        {
            return Result<EmergencyAlertDto>.Failure("Emergency alert not found.");
        }

        if (entity.Status != EmergencyAlertStatus.Active)
        {
            return Result<EmergencyAlertDto>.Failure("This alert has already been resolved.");
        }

        entity.Status = EmergencyAlertStatus.Resolved;
        entity.ResolvedByEmployeeId = resolverId;
        entity.ResolvedAtUtc = DateTime.UtcNow;
        entity.ResolutionNotes = request.ResolutionNotes;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<EmergencyAlertDto>.Success(ToDto(entity, entity.Employee!.FullName));
    }

    private static EmergencyAlertDto ToDto(EmergencyAlert entity, string employeeName) => new(
        entity.Id,
        entity.EmployeeId,
        employeeName,
        entity.Location,
        entity.Message,
        entity.Status,
        entity.TriggeredAtUtc,
        entity.ResolvedAtUtc,
        entity.ResolutionNotes);
}
