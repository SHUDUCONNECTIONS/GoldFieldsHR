using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Notifications;
using GoldFieldsHR.Application.Ppe;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Ppe;

public class PpeService(ApplicationDbContext dbContext, INotificationService notificationService) : IPpeService
{
    public async Task<Result<PpeRequestDto>> SubmitAsync(
        Guid employeeId, SubmitPpeRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity < 1 || request.Quantity > 20)
        {
            return Result<PpeRequestDto>.Failure("Quantity must be between 1 and 20.");
        }

        var employee = await dbContext.Employees.FindAsync([employeeId], cancellationToken);
        if (employee is null)
        {
            return Result<PpeRequestDto>.Failure("Employee profile not found.");
        }

        var entity = new PpeRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            ItemType = request.ItemType,
            Size = request.Size,
            Quantity = request.Quantity,
            Reason = request.Reason,
        };

        dbContext.PpeRequests.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<PpeRequestDto>.Success(ToDto(entity, employee.FullName));
    }

    public async Task<IReadOnlyList<PpeRequestDto>> GetMyRequestsAsync(
        Guid employeeId, CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.PpeRequests
            .Include(r => r.Employee)
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(r => ToDto(r, r.Employee!.FullName)).ToList();
    }

    public async Task<IReadOnlyList<PpeRequestDto>> GetPendingApprovalsAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.PpeRequests
            .Include(r => r.Employee)
            .Where(r => r.Status == PpeRequestStatus.Pending)
            .OrderBy(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(r => ToDto(r, r.Employee!.FullName)).ToList();
    }

    public async Task<IReadOnlyList<PpeRequestDto>> GetAwaitingIssueAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.PpeRequests
            .Include(r => r.Employee)
            .Where(r => r.Status == PpeRequestStatus.Approved)
            .OrderBy(r => r.ReviewedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(r => ToDto(r, r.Employee!.FullName)).ToList();
    }

    public async Task<Result<PpeRequestDto>> ReviewAsync(
        Guid requestId, Guid reviewerId, ReviewPpeRequest review, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.PpeRequests
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (entity is null)
        {
            return Result<PpeRequestDto>.Failure("PPE request not found.");
        }

        if (entity.Status != PpeRequestStatus.Pending)
        {
            return Result<PpeRequestDto>.Failure("This request has already been reviewed.");
        }

        entity.ReviewerId = reviewerId;
        entity.ReviewedAtUtc = DateTime.UtcNow;
        entity.Status = review.Approve ? PpeRequestStatus.Approved : PpeRequestStatus.Rejected;
        entity.RejectionReason = review.Approve ? null : review.RejectionReason;

        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.CreateAsync(
            entity.EmployeeId,
            review.Approve
                ? $"Your PPE request ({entity.ItemType} x{entity.Quantity}) was approved and is awaiting issue."
                : $"Your PPE request ({entity.ItemType} x{entity.Quantity}) was rejected.",
            "/ppe",
            cancellationToken);

        return Result<PpeRequestDto>.Success(ToDto(entity, entity.Employee!.FullName));
    }

    public async Task<Result<PpeRequestDto>> MarkIssuedAsync(
        Guid requestId, Guid issuerId, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.PpeRequests
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (entity is null)
        {
            return Result<PpeRequestDto>.Failure("PPE request not found.");
        }

        if (entity.Status != PpeRequestStatus.Approved)
        {
            return Result<PpeRequestDto>.Failure("Only approved requests can be marked as issued.");
        }

        entity.Status = PpeRequestStatus.Issued;
        entity.IssuedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.CreateAsync(
            entity.EmployeeId,
            $"Your PPE request ({entity.ItemType} x{entity.Quantity}) has been issued.",
            "/ppe",
            cancellationToken);

        return Result<PpeRequestDto>.Success(ToDto(entity, entity.Employee!.FullName));
    }

    private static PpeRequestDto ToDto(PpeRequest entity, string employeeName) => new(
        entity.Id,
        entity.EmployeeId,
        employeeName,
        entity.ItemType,
        entity.Size,
        entity.Quantity,
        entity.Reason,
        entity.Status,
        entity.CreatedAtUtc,
        entity.ReviewedAtUtc,
        entity.RejectionReason,
        entity.IssuedAtUtc);
}
