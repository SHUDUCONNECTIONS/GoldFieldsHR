using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.Account;

public record ProfileDto(
    Guid EmployeeId,
    string FullName,
    string Email,
    string EmployeeNumber,
    string JobTitle,
    EmployeeRole Role,
    string SiteName);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record SetSignatureRequest(string SignaturePngBase64);

public record SignatureDto(bool HasSignature, string? SignaturePngBase64, DateTime? UpdatedAtUtc);
