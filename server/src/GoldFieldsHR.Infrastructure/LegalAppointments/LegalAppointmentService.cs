using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.LegalAppointments;
using GoldFieldsHR.Application.Notifications;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.LegalAppointments;

public class LegalAppointmentService(ApplicationDbContext dbContext, INotificationService notificationService) : ILegalAppointmentService
{
    public async Task<Result<LegalAppointmentDto>> SubmitAsync(
        Guid employeeId, SubmitLegalAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ValidTo < request.ValidFrom)
        {
            return Result<LegalAppointmentDto>.Failure("Valid-to date cannot be before the valid-from date.");
        }

        var employee = await dbContext.Employees.FindAsync([employeeId], cancellationToken);
        if (employee is null)
        {
            return Result<LegalAppointmentDto>.Failure("Employee profile not found.");
        }

        var entity = new LegalAppointment
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            AppointmentType = request.AppointmentType,
            AppointedBy = request.AppointedBy,
            Description = request.Description,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
        };

        dbContext.LegalAppointments.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<LegalAppointmentDto>.Success(ToDto(entity, employee.FullName));
    }

    public async Task<IReadOnlyList<LegalAppointmentDto>> GetMyAppointmentsAsync(
        Guid employeeId, CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.LegalAppointments
            .Include(p => p.Employee)
            .Where(p => p.EmployeeId == employeeId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(p => ToDto(p, p.Employee!.FullName)).ToList();
    }

    public async Task<IReadOnlyList<LegalAppointmentDto>> GetPendingApprovalsAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.LegalAppointments
            .Include(p => p.Employee)
            .Where(p => p.Status == LegalAppointmentStatus.Pending)
            .OrderBy(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(p => ToDto(p, p.Employee!.FullName)).ToList();
    }

    public async Task<IReadOnlyList<LegalAppointmentDto>> GetActiveAppointmentsAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.LegalAppointments
            .Include(p => p.Employee)
            .Where(p => p.Status == LegalAppointmentStatus.Active)
            .OrderBy(p => p.ValidFrom)
            .ToListAsync(cancellationToken);

        return entities.Select(p => ToDto(p, p.Employee!.FullName)).ToList();
    }

    public async Task<Result<LegalAppointmentDto>> ReviewAsync(
        Guid appointmentId, Guid reviewerId, ReviewLegalAppointmentRequest review, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.LegalAppointments
            .Include(p => p.Employee)
            .FirstOrDefaultAsync(p => p.Id == appointmentId, cancellationToken);

        if (entity is null)
        {
            return Result<LegalAppointmentDto>.Failure("Legal appointment not found.");
        }

        if (entity.Status != LegalAppointmentStatus.Pending)
        {
            return Result<LegalAppointmentDto>.Failure("This appointment has already been reviewed.");
        }

        entity.ReviewerId = reviewerId;
        entity.ReviewedAtUtc = DateTime.UtcNow;
        entity.Status = review.Approve ? LegalAppointmentStatus.Active : LegalAppointmentStatus.Rejected;
        entity.RejectionReason = review.Approve ? null : review.RejectionReason;

        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.CreateAsync(
            entity.EmployeeId,
            review.Approve
                ? $"Your {entity.AppointmentType} appointment was approved."
                : $"Your {entity.AppointmentType} appointment was rejected.",
            "/legal-appointments",
            cancellationToken);

        return Result<LegalAppointmentDto>.Success(ToDto(entity, entity.Employee!.FullName));
    }

    public async Task<Result<LegalAppointmentDto>> RevokeAsync(
        Guid appointmentId, RevokeLegalAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.LegalAppointments
            .Include(p => p.Employee)
            .FirstOrDefaultAsync(p => p.Id == appointmentId, cancellationToken);

        if (entity is null)
        {
            return Result<LegalAppointmentDto>.Failure("Legal appointment not found.");
        }

        if (entity.Status != LegalAppointmentStatus.Active)
        {
            return Result<LegalAppointmentDto>.Failure("Only active appointments can be revoked.");
        }

        entity.Status = LegalAppointmentStatus.Revoked;
        entity.RevokedAtUtc = DateTime.UtcNow;
        entity.RevokedNotes = request.RevokedNotes;

        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.CreateAsync(
            entity.EmployeeId,
            $"Your {entity.AppointmentType} appointment has been revoked.",
            "/legal-appointments",
            cancellationToken);

        return Result<LegalAppointmentDto>.Success(ToDto(entity, entity.Employee!.FullName));
    }

    private static LegalAppointmentDto ToDto(LegalAppointment entity, string employeeName) => new(
        entity.Id,
        entity.EmployeeId,
        employeeName,
        entity.AppointmentType,
        entity.AppointedBy,
        entity.Description,
        entity.ValidFrom,
        entity.ValidTo,
        entity.Status,
        entity.CreatedAtUtc,
        entity.ReviewedAtUtc,
        entity.RejectionReason,
        entity.RevokedAtUtc,
        entity.RevokedNotes);
}
