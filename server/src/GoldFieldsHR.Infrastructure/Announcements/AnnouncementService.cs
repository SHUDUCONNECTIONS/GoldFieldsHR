using GoldFieldsHR.Application.Announcements;
using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Notifications;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Announcements;

public class AnnouncementService(ApplicationDbContext dbContext, INotificationService notificationService) : IAnnouncementService
{
    public async Task<Result<AnnouncementDto>> CreateAsync(
        Guid posterEmployeeId, CreateAnnouncementRequest request, CancellationToken cancellationToken = default)
    {
        var poster = await dbContext.Employees.FindAsync([posterEmployeeId], cancellationToken);
        if (poster is null)
        {
            return Result<AnnouncementDto>.Failure("Employee profile not found.");
        }

        var entity = new Announcement
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Body = request.Body,
            PostedByEmployeeId = posterEmployeeId,
        };

        dbContext.Announcements.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        var recipientIds = await dbContext.Employees
            .Where(e => e.IsActive && e.Id != posterEmployeeId)
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        await notificationService.CreateForManyAsync(
            recipientIds, $"New announcement: {entity.Title}", "/announcements", cancellationToken);

        return Result<AnnouncementDto>.Success(new AnnouncementDto(
            entity.Id, entity.Title, entity.Body, poster.FullName, entity.CreatedAtUtc));
    }

    public async Task<IReadOnlyList<AnnouncementDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var announcements = await dbContext.Announcements
            .Include(a => a.PostedByEmployee)
            .OrderByDescending(a => a.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return announcements
            .Select(a => new AnnouncementDto(a.Id, a.Title, a.Body, a.PostedByEmployee!.FullName, a.CreatedAtUtc))
            .ToList();
    }
}
