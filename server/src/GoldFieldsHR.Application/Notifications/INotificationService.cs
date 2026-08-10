using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Notifications;

public interface INotificationService
{
    Task CreateAsync(Guid recipientEmployeeId, string message, string? link, CancellationToken cancellationToken = default);

    Task CreateForManyAsync(IEnumerable<Guid> recipientEmployeeIds, string message, string? link, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationDto>> GetMineAsync(Guid employeeId, CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(Guid employeeId, CancellationToken cancellationToken = default);

    Task<Result<bool>> MarkAsReadAsync(Guid notificationId, Guid employeeId, CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(Guid employeeId, CancellationToken cancellationToken = default);
}
