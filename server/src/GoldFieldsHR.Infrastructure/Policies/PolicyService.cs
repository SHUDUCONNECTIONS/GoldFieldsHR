using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Notifications;
using GoldFieldsHR.Application.Policies;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Policies;

public class PolicyService(ApplicationDbContext dbContext, INotificationService notificationService) : IPolicyService
{
    public async Task<Result<PolicyDto>> CreateAsync(
        Guid publisherEmployeeId, CreatePolicyRequest request, CancellationToken cancellationToken = default)
    {
        var publisher = await dbContext.Employees.FindAsync([publisherEmployeeId], cancellationToken);
        if (publisher is null)
        {
            return Result<PolicyDto>.Failure("Employee profile not found.");
        }

        var entity = new Policy
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            PublishedByEmployeeId = publisherEmployeeId,
        };

        dbContext.Policies.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        var recipientIds = await dbContext.Employees
            .Where(e => e.IsActive && e.Id != publisherEmployeeId)
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        await notificationService.CreateForManyAsync(
            recipientIds, $"New policy published: {entity.Title}. Please review and acknowledge.", "/policies", cancellationToken);

        return Result<PolicyDto>.Success(new PolicyDto(
            entity.Id, entity.Title, entity.Content, publisher.FullName, entity.PublishedAtUtc,
            false, null, 0));
    }

    public async Task<IReadOnlyList<PolicyDto>> GetAllAsync(
        Guid requestingEmployeeId, CancellationToken cancellationToken = default)
    {
        var policies = await dbContext.Policies
            .Include(p => p.PublishedByEmployee)
            .Include(p => p.Acknowledgments)
            .OrderByDescending(p => p.PublishedAtUtc)
            .ToListAsync(cancellationToken);

        return policies.Select(p =>
        {
            var myAck = p.Acknowledgments.FirstOrDefault(a => a.EmployeeId == requestingEmployeeId);
            return new PolicyDto(
                p.Id,
                p.Title,
                p.Content,
                p.PublishedByEmployee!.FullName,
                p.PublishedAtUtc,
                myAck is not null,
                myAck?.AcknowledgedAtUtc,
                p.Acknowledgments.Count);
        }).ToList();
    }

    public async Task<Result<PolicyDto>> AcknowledgeAsync(
        Guid policyId, Guid employeeId, CancellationToken cancellationToken = default)
    {
        var policy = await dbContext.Policies
            .Include(p => p.PublishedByEmployee)
            .Include(p => p.Acknowledgments)
            .FirstOrDefaultAsync(p => p.Id == policyId, cancellationToken);

        if (policy is null)
        {
            return Result<PolicyDto>.Failure("Policy not found.");
        }

        if (policy.Acknowledgments.Any(a => a.EmployeeId == employeeId))
        {
            return Result<PolicyDto>.Failure("You have already acknowledged this policy.");
        }

        var acknowledgment = new PolicyAcknowledgment
        {
            Id = Guid.NewGuid(),
            PolicyId = policyId,
            EmployeeId = employeeId,
        };

        dbContext.PolicyAcknowledgments.Add(acknowledgment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<PolicyDto>.Success(new PolicyDto(
            policy.Id,
            policy.Title,
            policy.Content,
            policy.PublishedByEmployee!.FullName,
            policy.PublishedAtUtc,
            true,
            acknowledgment.AcknowledgedAtUtc,
            policy.Acknowledgments.Count));
    }

    public async Task<IReadOnlyList<PolicyAcknowledgmentDto>> GetAcknowledgmentsAsync(
        Guid policyId, CancellationToken cancellationToken = default)
    {
        var acknowledgments = await dbContext.PolicyAcknowledgments
            .Include(a => a.Employee)
            .Where(a => a.PolicyId == policyId)
            .OrderBy(a => a.AcknowledgedAtUtc)
            .ToListAsync(cancellationToken);

        return acknowledgments
            .Select(a => new PolicyAcknowledgmentDto(a.EmployeeId, a.Employee!.FullName, a.AcknowledgedAtUtc))
            .ToList();
    }
}
