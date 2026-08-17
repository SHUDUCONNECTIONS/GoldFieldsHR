namespace GoldFieldsHR.Application.WorkShift;

public record CreateScheduleDocumentRequest(string Title);

public record PostedScheduleDocumentDto(
    Guid Id,
    string Title,
    Guid PostedByEmployeeId,
    string PostedByName,
    DateTime PostedAtUtc);
