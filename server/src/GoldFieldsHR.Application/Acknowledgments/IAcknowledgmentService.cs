using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.Acknowledgments;

public interface IAcknowledgmentService
{
    Task<Result<AcknowledgmentDto>> AcknowledgeAsync(
        AcknowledgmentEntityType entityType, Guid entityId, Guid employeeId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<AcknowledgmentDto>>> GetForEntityAsync(
        AcknowledgmentEntityType entityType, Guid entityId, Guid requesterId, CancellationToken cancellationToken = default);
}
