using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Account;

public interface IAccountService
{
    Task<ProfileDto?> GetProfileAsync(Guid employeeId, CancellationToken cancellationToken = default);

    Task<Result<bool>> ChangePasswordAsync(
        Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
