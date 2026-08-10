using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Auth;

public interface IAuthService
{
    Task<AuthResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<AuthResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<string?> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);

    Task<AuthResult<bool>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);

    Task<AuthResult<AuthResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);

    Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);
}
