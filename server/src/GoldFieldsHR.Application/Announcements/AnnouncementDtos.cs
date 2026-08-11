namespace GoldFieldsHR.Application.Announcements;

public record CreateAnnouncementRequest(string Title, string Body);

public record AnnouncementDto(
    Guid Id,
    string Title,
    string Body,
    string PostedByName,
    DateTime CreatedAtUtc);
