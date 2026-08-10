using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Notifications;
using GoldFieldsHR.Application.WorkShift;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.WorkShift;

public class WorkShiftService(ApplicationDbContext dbContext, INotificationService notificationService) : IWorkShiftService
{
    public async Task<Result<ShiftChangeRequestDto>> SubmitAsync(
        Guid employeeId, SubmitShiftChangeRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees.FindAsync([employeeId], cancellationToken);
        if (employee is null)
        {
            return Result<ShiftChangeRequestDto>.Failure("Employee profile not found.");
        }

        var entity = new ShiftChangeRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            RequestedShiftDate = request.RequestedShiftDate,
            RequestedShiftType = request.RequestedShiftType,
            Reason = request.Reason,
            Comments = request.Comments,
        };

        dbContext.ShiftChangeRequests.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<ShiftChangeRequestDto>.Success(ToDto(entity, employee.FullName));
    }

    public async Task<IReadOnlyList<ShiftChangeRequestDto>> GetMyRequestsAsync(
        Guid employeeId, CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.ShiftChangeRequests
            .Include(r => r.Employee)
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(r => ToDto(r, r.Employee!.FullName)).ToList();
    }

    public async Task<IReadOnlyList<ShiftChangeRequestDto>> GetPendingLineManagerApprovalsAsync(
        Guid reviewerId, CancellationToken cancellationToken = default)
    {
        var entities = await GetEntitiesByStatusAsync(ShiftRequestStatus.PendingLineManagerApproval, cancellationToken);

        // Direct reports surface first; every other pending request still follows as a site-wide fallback.
        return entities
            .Select(r => ToDto(r, r.Employee!.FullName, r.Employee.ManagerId == reviewerId))
            .OrderByDescending(dto => dto.IsDirectReport)
            .ToList();
    }

    public async Task<IReadOnlyList<ShiftChangeRequestDto>> GetPendingHRApprovalsAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await GetEntitiesByStatusAsync(ShiftRequestStatus.PendingHRApproval, cancellationToken);
        return entities.Select(r => ToDto(r, r.Employee!.FullName)).ToList();
    }

    public async Task<Result<ShiftChangeRequestDto>> LineManagerReviewAsync(
        Guid requestId, Guid reviewerId, ReviewShiftChangeRequest review, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.ShiftChangeRequests
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (entity is null)
        {
            return Result<ShiftChangeRequestDto>.Failure("Shift change request not found.");
        }

        if (entity.Status != ShiftRequestStatus.PendingLineManagerApproval)
        {
            return Result<ShiftChangeRequestDto>.Failure("This request is no longer awaiting line manager approval.");
        }

        entity.LineManagerReviewerId = reviewerId;
        entity.LineManagerReviewedAtUtc = DateTime.UtcNow;
        entity.Status = review.Approve ? ShiftRequestStatus.PendingHRApproval : ShiftRequestStatus.Rejected;
        entity.RejectionReason = review.Approve ? null : review.RejectionReason;

        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.CreateAsync(
            entity.EmployeeId,
            review.Approve
                ? $"Your shift change request for {entity.RequestedShiftDate:d MMM} was approved by your Line Manager and is now awaiting HR approval."
                : $"Your shift change request for {entity.RequestedShiftDate:d MMM} was rejected by your Line Manager.",
            "/work-shift",
            cancellationToken);

        return Result<ShiftChangeRequestDto>.Success(ToDto(entity, entity.Employee!.FullName));
    }

    public async Task<Result<ShiftChangeRequestDto>> HRReviewAsync(
        Guid requestId, Guid reviewerId, ReviewShiftChangeRequest review, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.ShiftChangeRequests
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (entity is null)
        {
            return Result<ShiftChangeRequestDto>.Failure("Shift change request not found.");
        }

        if (entity.Status != ShiftRequestStatus.PendingHRApproval)
        {
            return Result<ShiftChangeRequestDto>.Failure("This request is no longer awaiting HR approval.");
        }

        entity.HRReviewerId = reviewerId;
        entity.HRReviewedAtUtc = DateTime.UtcNow;
        entity.Status = review.Approve ? ShiftRequestStatus.Approved : ShiftRequestStatus.Rejected;
        entity.RejectionReason = review.Approve ? null : review.RejectionReason;

        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.CreateAsync(
            entity.EmployeeId,
            review.Approve
                ? $"Your shift change request for {entity.RequestedShiftDate:d MMM} was fully approved."
                : $"Your shift change request for {entity.RequestedShiftDate:d MMM} was rejected by HR.",
            "/work-shift",
            cancellationToken);

        return Result<ShiftChangeRequestDto>.Success(ToDto(entity, entity.Employee!.FullName));
    }

    private async Task<List<ShiftChangeRequest>> GetEntitiesByStatusAsync(
        ShiftRequestStatus status, CancellationToken cancellationToken)
    {
        return await dbContext.ShiftChangeRequests
            .Include(r => r.Employee)
            .Where(r => r.Status == status)
            .OrderBy(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    private static ShiftChangeRequestDto ToDto(ShiftChangeRequest entity, string employeeName, bool isDirectReport = false) => new(
        entity.Id,
        entity.EmployeeId,
        employeeName,
        entity.RequestedShiftDate,
        entity.RequestedShiftType,
        entity.Reason,
        entity.Comments,
        entity.Status,
        entity.CreatedAtUtc,
        entity.LineManagerReviewedAtUtc,
        entity.HRReviewedAtUtc,
        entity.RejectionReason,
        isDirectReport);
}
