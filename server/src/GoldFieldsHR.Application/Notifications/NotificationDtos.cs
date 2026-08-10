namespace GoldFieldsHR.Application.Notifications;

public record NotificationDto(Guid Id, string Message, string? Link, bool IsRead, DateTime CreatedAtUtc);
