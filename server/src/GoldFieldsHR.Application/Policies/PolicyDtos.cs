namespace GoldFieldsHR.Application.Policies;

public record CreatePolicyRequest(string Title, string Content);

public record PolicyDto(
    Guid Id,
    string Title,
    string Content,
    string PublishedByName,
    DateTime PublishedAtUtc,
    bool AcknowledgedByMe,
    DateTime? AcknowledgedAtUtc,
    int AcknowledgmentCount);

public record PolicyAcknowledgmentDto(Guid EmployeeId, string EmployeeName, DateTime AcknowledgedAtUtc);
