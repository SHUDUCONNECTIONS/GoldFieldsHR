using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.Employees;

public record EmployeeSummaryDto(
    Guid Id,
    string EmployeeNumber,
    string FullName,
    string Email,
    string JobTitle,
    EmployeeRole Role,
    EmployeeRole? RequestedRole,
    string SiteName,
    bool IsActive,
    DateTime CreatedAtUtc,
    Guid? ManagerId,
    string? ManagerName);

public record EmployeeDirectoryQuery(
    string? Search,
    EmployeeRole? Role,
    bool? IsActive,
    int Page = 1,
    int PageSize = 25,
    bool? HasRequestedRole = null);

public record SetEmployeeActiveStatusRequest(bool IsActive);

public record SetEmployeeManagerRequest(string? ManagerEmployeeNumber);

public record SetEmployeeRoleRequest(EmployeeRole Role);

public record EmployeeLiteDto(Guid Id, string FullName, string JobTitle, string SiteName);
