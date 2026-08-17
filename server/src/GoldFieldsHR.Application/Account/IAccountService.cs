using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Account;

public interface IAccountService
{
    Task<ProfileDto?> GetProfileAsync(Guid employeeId, CancellationToken cancellationToken = default);

    Task<Result<bool>> ChangePasswordAsync(
        Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);

    Task<Result<SignatureDto>> GetSignatureAsync(Guid employeeId, CancellationToken cancellationToken = default);

    Task<Result<SignatureDto>> SetSignatureAsync(
        Guid employeeId, SetSignatureRequest request, CancellationToken cancellationToken = default);
}
