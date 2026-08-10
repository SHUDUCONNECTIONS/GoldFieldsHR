using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Performance;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Performance;

public class PerformanceService(ApplicationDbContext dbContext) : IPerformanceService
{
    public async Task<Result<PerformanceReviewDto>> CreateAsync(
        Guid reviewerEmployeeId, CreatePerformanceReviewRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Score is < 1 or > 5)
        {
            return Result<PerformanceReviewDto>.Failure("Score must be between 1 and 5.");
        }

        var reviewer = await dbContext.Employees.FindAsync([reviewerEmployeeId], cancellationToken);
        if (reviewer is null)
        {
            return Result<PerformanceReviewDto>.Failure("Employee profile not found.");
        }

        var recipient = await dbContext.Employees
            .FirstOrDefaultAsync(e => e.EmployeeNumber == request.EmployeeNumber, cancellationToken);
        if (recipient is null)
        {
            return Result<PerformanceReviewDto>.Failure($"No employee found with number '{request.EmployeeNumber}'.");
        }

        if (recipient.Id == reviewerEmployeeId)
        {
            return Result<PerformanceReviewDto>.Failure("You cannot review yourself.");
        }

        var entity = new PerformanceReview
        {
            Id = Guid.NewGuid(),
            EmployeeId = recipient.Id,
            ReviewerId = reviewerEmployeeId,
            PeriodLabel = request.PeriodLabel,
            Score = request.Score,
            Comments = request.Comments,
        };

        dbContext.PerformanceReviews.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<PerformanceReviewDto>.Success(ToDto(entity, recipient.FullName, reviewer.FullName));
    }

    public async Task<IReadOnlyList<PerformanceReviewDto>> GetMyReviewsAsync(
        Guid employeeId, CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.PerformanceReviews
            .Include(r => r.Employee)
            .Include(r => r.Reviewer)
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(r => ToDto(r, r.Employee!.FullName, r.Reviewer!.FullName)).ToList();
    }

    public async Task<IReadOnlyList<PerformanceReviewDto>> GetGivenByMeAsync(
        Guid reviewerEmployeeId, CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.PerformanceReviews
            .Include(r => r.Employee)
            .Include(r => r.Reviewer)
            .Where(r => r.ReviewerId == reviewerEmployeeId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(r => ToDto(r, r.Employee!.FullName, r.Reviewer!.FullName)).ToList();
    }

    public async Task<IReadOnlyList<PerformanceReviewDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.PerformanceReviews
            .Include(r => r.Employee)
            .Include(r => r.Reviewer)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(r => ToDto(r, r.Employee!.FullName, r.Reviewer!.FullName)).ToList();
    }

    private static PerformanceReviewDto ToDto(PerformanceReview entity, string employeeName, string reviewerName) => new(
        entity.Id,
        entity.EmployeeId,
        employeeName,
        entity.ReviewerId,
        reviewerName,
        entity.PeriodLabel,
        entity.Score,
        entity.Comments,
        entity.CreatedAtUtc);
}
