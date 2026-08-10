using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Notifications;
using GoldFieldsHR.Application.Permits;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Permits;

public class PermitService(ApplicationDbContext dbContext, INotificationService notificationService) : IPermitService
{
    public async Task<Result<WorkPermitDto>> SubmitAsync(
        Guid employeeId, SubmitPermitRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ValidTo < request.ValidFrom)
        {
            return Result<WorkPermitDto>.Failure("Valid-to date cannot be before the valid-from date.");
        }

        var employee = await dbContext.Employees.FindAsync([employeeId], cancellationToken);
        if (employee is null)
        {
            return Result<WorkPermitDto>.Failure("Employee profile not found.");
        }

        var entity = new WorkPermit
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            PermitType = request.PermitType,
            Location = request.Location,
            Description = request.Description,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
        };

        dbContext.WorkPermits.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<WorkPermitDto>.Success(ToDto(entity, employee.FullName));
    }

    public async Task<IReadOnlyList<WorkPermitDto>> GetMyPermitsAsync(
        Guid employeeId, CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.WorkPermits
            .Include(p => p.Employee)
            .Where(p => p.EmployeeId == employeeId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(p => ToDto(p, p.Employee!.FullName)).ToList();
    }

    public async Task<IReadOnlyList<WorkPermitDto>> GetPendingApprovalsAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.WorkPermits
            .Include(p => p.Employee)
            .Where(p => p.Status == PermitStatus.Pending)
            .OrderBy(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(p => ToDto(p, p.Employee!.FullName)).ToList();
    }

    public async Task<IReadOnlyList<WorkPermitDto>> GetOpenPermitsAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.WorkPermits
            .Include(p => p.Employee)
            .Where(p => p.Status == PermitStatus.Approved)
            .OrderBy(p => p.ValidFrom)
            .ToListAsync(cancellationToken);

        return entities.Select(p => ToDto(p, p.Employee!.FullName)).ToList();
    }

    public async Task<Result<WorkPermitDto>> ReviewAsync(
        Guid permitId, Guid reviewerId, ReviewPermitRequest review, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.WorkPermits
            .Include(p => p.Employee)
            .FirstOrDefaultAsync(p => p.Id == permitId, cancellationToken);

        if (entity is null)
        {
            return Result<WorkPermitDto>.Failure("Work permit not found.");
        }

        if (entity.Status != PermitStatus.Pending)
        {
            return Result<WorkPermitDto>.Failure("This permit has already been reviewed.");
        }

        entity.ReviewerId = reviewerId;
        entity.ReviewedAtUtc = DateTime.UtcNow;
        entity.Status = review.Approve ? PermitStatus.Approved : PermitStatus.Rejected;
        entity.RejectionReason = review.Approve ? null : review.RejectionReason;

        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.CreateAsync(
            entity.EmployeeId,
            review.Approve
                ? $"Your {entity.PermitType} permit for {entity.Location} was approved."
                : $"Your {entity.PermitType} permit for {entity.Location} was rejected.",
            "/permits",
            cancellationToken);

        return Result<WorkPermitDto>.Success(ToDto(entity, entity.Employee!.FullName));
    }

    public async Task<Result<WorkPermitDto>> CloseAsync(
        Guid permitId, ClosePermitRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.WorkPermits
            .Include(p => p.Employee)
            .FirstOrDefaultAsync(p => p.Id == permitId, cancellationToken);

        if (entity is null)
        {
            return Result<WorkPermitDto>.Failure("Work permit not found.");
        }

        if (entity.Status != PermitStatus.Approved)
        {
            return Result<WorkPermitDto>.Failure("Only approved permits can be closed out.");
        }

        entity.Status = PermitStatus.Closed;
        entity.ClosedAtUtc = DateTime.UtcNow;
        entity.ClosedNotes = request.ClosedNotes;

        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.CreateAsync(
            entity.EmployeeId,
            $"Your {entity.PermitType} permit for {entity.Location} has been closed out.",
            "/permits",
            cancellationToken);

        return Result<WorkPermitDto>.Success(ToDto(entity, entity.Employee!.FullName));
    }

    private static WorkPermitDto ToDto(WorkPermit entity, string employeeName) => new(
        entity.Id,
        entity.EmployeeId,
        employeeName,
        entity.PermitType,
        entity.Location,
        entity.Description,
        entity.ValidFrom,
        entity.ValidTo,
        entity.Status,
        entity.CreatedAtUtc,
        entity.ReviewedAtUtc,
        entity.RejectionReason,
        entity.ClosedAtUtc,
        entity.ClosedNotes);
}
