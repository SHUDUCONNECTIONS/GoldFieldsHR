using GoldFieldsHR.Application.Acknowledgments;
using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Acknowledgments;

public class AcknowledgmentService(ApplicationDbContext dbContext) : IAcknowledgmentService
{
    public async Task<Result<AcknowledgmentDto>> AcknowledgeAsync(
        AcknowledgmentEntityType entityType, Guid entityId, Guid employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees.FindAsync([employeeId], cancellationToken);
        if (employee is null)
        {
            return Result<AcknowledgmentDto>.Failure("Employee profile not found.");
        }

        if (employee.Role is not (EmployeeRole.HR or EmployeeRole.Executive))
        {
            return Result<AcknowledgmentDto>.Failure("You are not authorized to acknowledge this record.");
        }

        if (!await EntityExistsAsync(entityType, entityId, cancellationToken))
        {
            return Result<AcknowledgmentDto>.Failure("The record you're acknowledging could not be found.");
        }

        var existing = await dbContext.Acknowledgments
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(
                a => a.EntityType == entityType && a.EntityId == entityId && a.EmployeeId == employeeId,
                cancellationToken);

        if (existing is not null)
        {
            return Result<AcknowledgmentDto>.Success(ToDto(existing, employee.FullName));
        }

        var acknowledgment = new Acknowledgment
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            EmployeeId = employeeId,
        };

        dbContext.Acknowledgments.Add(acknowledgment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<AcknowledgmentDto>.Success(ToDto(acknowledgment, employee.FullName));
    }

    public async Task<Result<IReadOnlyList<AcknowledgmentDto>>> GetForEntityAsync(
        AcknowledgmentEntityType entityType, Guid entityId, Guid requesterId, CancellationToken cancellationToken = default)
    {
        var requester = await dbContext.Employees.FindAsync([requesterId], cancellationToken);
        if (requester is null)
        {
            return Result<IReadOnlyList<AcknowledgmentDto>>.Failure("Employee profile not found.");
        }

        if (requester.Role is not (EmployeeRole.HR or EmployeeRole.Executive or EmployeeRole.SafetyOfficer))
        {
            return Result<IReadOnlyList<AcknowledgmentDto>>.Failure("You are not authorized to view acknowledgments on this record.");
        }

        if (!await EntityExistsAsync(entityType, entityId, cancellationToken))
        {
            return Result<IReadOnlyList<AcknowledgmentDto>>.Failure("Record not found.");
        }

        var acknowledgments = await dbContext.Acknowledgments
            .Include(a => a.Employee)
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderBy(a => a.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<AcknowledgmentDto>>.Success(
            acknowledgments.Select(a => ToDto(a, a.Employee!.FullName)).ToList());
    }

    private Task<bool> EntityExistsAsync(AcknowledgmentEntityType entityType, Guid entityId, CancellationToken cancellationToken) =>
        entityType switch
        {
            AcknowledgmentEntityType.IncidentReport => dbContext.IncidentReports.AnyAsync(i => i.Id == entityId, cancellationToken),
            AcknowledgmentEntityType.PreShiftSafetyCheck => dbContext.PreShiftSafetyChecks.AnyAsync(c => c.Id == entityId, cancellationToken),
            AcknowledgmentEntityType.PpeRequest => dbContext.PpeRequests.AnyAsync(p => p.Id == entityId, cancellationToken),
            AcknowledgmentEntityType.LegalAppointment => dbContext.LegalAppointments.AnyAsync(l => l.Id == entityId, cancellationToken),
            _ => Task.FromResult(false),
        };

    private static AcknowledgmentDto ToDto(Acknowledgment acknowledgment, string employeeName) => new(
        acknowledgment.Id,
        acknowledgment.EntityType,
        acknowledgment.EntityId,
        acknowledgment.EmployeeId,
        employeeName,
        acknowledgment.CreatedAtUtc);
}
