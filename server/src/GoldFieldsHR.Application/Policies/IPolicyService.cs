using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Policies;

public interface IPolicyService
{
    Task<Result<PolicyDto>> CreateAsync(
        Guid publisherEmployeeId, CreatePolicyRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PolicyDto>> GetAllAsync(Guid requestingEmployeeId, CancellationToken cancellationToken = default);

    Task<Result<PolicyDto>> AcknowledgeAsync(
        Guid policyId, Guid employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PolicyAcknowledgmentDto>> GetAcknowledgmentsAsync(
        Guid policyId, CancellationToken cancellationToken = default);
}
