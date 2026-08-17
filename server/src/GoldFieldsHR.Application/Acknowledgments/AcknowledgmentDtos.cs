using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.Acknowledgments;

public record AcknowledgmentDto(
    Guid Id,
    AcknowledgmentEntityType EntityType,
    Guid EntityId,
    Guid EmployeeId,
    string EmployeeName,
    DateTime CreatedAtUtc);
