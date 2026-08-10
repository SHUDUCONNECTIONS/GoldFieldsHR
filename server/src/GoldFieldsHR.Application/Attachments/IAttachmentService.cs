using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.Attachments;

public interface IAttachmentService
{
    Task<Result<AttachmentDto>> UploadAsync(
        AttachmentEntityType entityType, Guid entityId, Guid uploaderEmployeeId,
        UploadAttachmentRequest request, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<AttachmentDto>>> GetForEntityAsync(
        AttachmentEntityType entityType, Guid entityId, Guid requesterId, CancellationToken cancellationToken = default);

    Task<Result<AttachmentContentDto>> DownloadAsync(
        Guid attachmentId, Guid requesterId, CancellationToken cancellationToken = default);
}
