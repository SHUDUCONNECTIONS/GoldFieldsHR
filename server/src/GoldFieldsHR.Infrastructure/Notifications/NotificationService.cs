using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Notifications;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Notifications;

public class NotificationService(ApplicationDbContext dbContext) : INotificationService
{
    public async Task CreateAsync(Guid recipientEmployeeId, string message, string? link, CancellationToken cancellationToken = default)
    {
        dbContext.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            RecipientEmployeeId = recipientEmployeeId,
            Message = message,
            Link = link,
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateForManyAsync(
        IEnumerable<Guid> recipientEmployeeIds, string message, string? link, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        dbContext.Notifications.AddRange(recipientEmployeeIds.Select(id => new Notification
        {
            Id = Guid.NewGuid(),
            RecipientEmployeeId = id,
            Message = message,
            Link = link,
            CreatedAtUtc = now,
        }));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationDto>> GetMineAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var notifications = await dbContext.Notifications
            .Where(n => n.RecipientEmployeeId == employeeId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(50)
            .ToListAsync(cancellationToken);

        return notifications.Select(ToDto).ToList();
    }

    public async Task<int> GetUnreadCountAsync(Guid employeeId, CancellationToken cancellationToken = default) =>
        await dbContext.Notifications.CountAsync(n => n.RecipientEmployeeId == employeeId && !n.IsRead, cancellationToken);

    public async Task<Result<bool>> MarkAsReadAsync(Guid notificationId, Guid employeeId, CancellationToken cancellationToken = default)
    {
        var notification = await dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId, cancellationToken);

        if (notification is null || notification.RecipientEmployeeId != employeeId)
        {
            return Result<bool>.Failure("Notification not found.");
        }

        notification.IsRead = true;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task MarkAllAsReadAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var unread = await dbContext.Notifications
            .Where(n => n.RecipientEmployeeId == employeeId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in unread)
        {
            notification.IsRead = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static NotificationDto ToDto(Notification notification) =>
        new(notification.Id, notification.Message, notification.Link, notification.IsRead, notification.CreatedAtUtc);
}
