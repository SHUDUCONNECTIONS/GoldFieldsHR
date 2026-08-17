using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Notifications;
using GoldFieldsHR.Application.WorkShift;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.WorkShift;

public class PostedScheduleDocumentService(ApplicationDbContext dbContext, INotificationService notificationService)
    : IPostedScheduleDocumentService
{
    public async Task<Result<PostedScheduleDocumentDto>> CreateAsync(
        Guid postedByEmployeeId, CreateScheduleDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var poster = await dbContext.Employees.FindAsync([postedByEmployeeId], cancellationToken);
        if (poster is null)
        {
            return Result<PostedScheduleDocumentDto>.Failure("Employee profile not found.");
        }

        var entity = new PostedScheduleDocument
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            PostedByEmployeeId = postedByEmployeeId,
        };

        dbContext.PostedScheduleDocuments.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        var recipientIds = await dbContext.Employees
            .Where(e => e.IsActive && e.Id != postedByEmployeeId)
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        await notificationService.CreateForManyAsync(
            recipientIds, $"HR posted a new schedule: {entity.Title}.", "/work-shift", cancellationToken);

        return Result<PostedScheduleDocumentDto>.Success(ToDto(entity, poster.FullName));
    }

    public async Task<IReadOnlyList<PostedScheduleDocumentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await dbContext.PostedScheduleDocuments
            .Include(d => d.PostedByEmployee)
            .OrderByDescending(d => d.PostedAtUtc)
            .ToListAsync(cancellationToken);

        return documents.Select(d => ToDto(d, d.PostedByEmployee!.FullName)).ToList();
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.PostedScheduleDocuments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<bool>.Failure("Schedule document not found.");
        }

        dbContext.PostedScheduleDocuments.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    private static PostedScheduleDocumentDto ToDto(PostedScheduleDocument entity, string postedByName) => new(
        entity.Id,
        entity.Title,
        entity.PostedByEmployeeId,
        postedByName,
        entity.PostedAtUtc);
}
