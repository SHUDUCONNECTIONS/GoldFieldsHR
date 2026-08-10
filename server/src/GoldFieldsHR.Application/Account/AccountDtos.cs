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
