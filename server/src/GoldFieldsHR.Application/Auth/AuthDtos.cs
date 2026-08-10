using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.Auth;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string EmployeeNumber,
    string JobTitle,
    EmployeeRole Role,
    Guid SiteId,
    string? ManagerEmployeeNumber = null);

public record LoginRequest(string Email, string Password);

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Email, string Token, string NewPassword);

public record RefreshTokenRequest(string RefreshToken);

public record LogoutRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    Guid EmployeeId,
    string FullName,
    EmployeeRole Role,
    string RefreshToken);
