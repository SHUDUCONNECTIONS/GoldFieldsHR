using GoldFieldsHR.Application.Attachments;
using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Policies;

public interface IPolicyService
{
    Task<Result<PolicyDto>> CreateAsync(
        Guid publisherEmployeeId, CreatePolicyRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PolicyDto>> GetAllAsync(Guid requestingEmployeeId, CancellationToken cancellationToken = default);

    Task<Result<PolicyDto>> AcknowledgeAsync(
        Guid policyId, Guid employeeId, AcknowledgePolicyRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PolicyAcknowledgmentDto>> GetAcknowledgmentsAsync(
        Guid policyId, CancellationToken cancellationToken = default);

    Task<Result<AttachmentContentDto>> DownloadSignedAttachmentAsync(
        Guid policyId, Guid employeeId, Guid attachmentId, Guid requesterId, CancellationToken cancellationToken = default);
}
