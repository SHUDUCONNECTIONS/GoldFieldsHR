using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Leave;
using GoldFieldsHR.Application.Notifications;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Leave;

public class LeaveService(ApplicationDbContext dbContext, INotificationService notificationService) : ILeaveService
{
    public async Task<Result<LeaveRequestDto>> SubmitAsync(
        Guid employeeId, SubmitLeaveRequest request, CancellationToken cancellationToken = default)
    {
        if (request.EndDate < request.StartDate)
        {
            return Result<LeaveRequestDto>.Failure("End date cannot be before the start date.");
        }

        var employee = await dbContext.Employees.FindAsync([employeeId], cancellationToken);
        if (employee is null)
        {
            return Result<LeaveRequestDto>.Failure("Employee profile not found.");
        }

        var entity = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            LeaveType = request.LeaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = request.Reason,
        };

        dbContext.LeaveRequests.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<LeaveRequestDto>.Success(ToDto(entity, employee.FullName));
    }

    public async Task<IReadOnlyList<LeaveRequestDto>> GetMyRequestsAsync(
        Guid employeeId, CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.LeaveRequests
            .Include(r => r.Employee)
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(r => ToDto(r, r.Employee!.FullName)).ToList();
    }

    public async Task<IReadOnlyList<LeaveRequestDto>> GetPendingApprovalsAsync(
        Guid reviewerId, CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.LeaveRequests
            .Include(r => r.Employee)
            .Where(r => r.Status == LeaveRequestStatus.Pending)
            .OrderBy(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        // Direct reports surface first; every other pending request still follows as a site-wide fallback.
        return entities
            .Select(r => ToDto(r, r.Employee!.FullName, r.Employee.ManagerId == reviewerId))
            .OrderByDescending(dto => dto.IsDirectReport)
            .ToList();
    }

    public async Task<Result<LeaveRequestDto>> ReviewAsync(
        Guid requestId, Guid reviewerId, ReviewLeaveRequest review, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.LeaveRequests
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (entity is null)
        {
            return Result<LeaveRequestDto>.Failure("Leave request not found.");
        }

        if (entity.Status != LeaveRequestStatus.Pending)
        {
            return Result<LeaveRequestDto>.Failure("This request has already been reviewed.");
        }

        entity.ReviewerId = reviewerId;
        entity.ReviewedAtUtc = DateTime.UtcNow;
        entity.Status = review.Approve ? LeaveRequestStatus.Approved : LeaveRequestStatus.Rejected;
        entity.RejectionReason = review.Approve ? null : review.RejectionReason;

        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.CreateAsync(
            entity.EmployeeId,
            review.Approve
                ? $"Your {entity.LeaveType} leave request ({entity.StartDate:d MMM} - {entity.EndDate:d MMM}) was approved."
                : $"Your {entity.LeaveType} leave request ({entity.StartDate:d MMM} - {entity.EndDate:d MMM}) was rejected.",
            "/leave",
            cancellationToken);

        return Result<LeaveRequestDto>.Success(ToDto(entity, entity.Employee!.FullName));
    }

    private static LeaveRequestDto ToDto(LeaveRequest entity, string employeeName, bool isDirectReport = false) => new(
        entity.Id,
        entity.EmployeeId,
        employeeName,
        entity.LeaveType,
        entity.StartDate,
        entity.EndDate,
        entity.EndDate.DayNumber - entity.StartDate.DayNumber + 1,
        entity.Reason,
        entity.Status,
        entity.CreatedAtUtc,
        entity.ReviewedAtUtc,
        entity.RejectionReason,
        isDirectReport);
}
