using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Employees;

public interface IEmployeeDirectoryService
{
    Task<IReadOnlyList<EmployeeSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<EmployeeSummaryDto>> SetActiveStatusAsync(
        Guid employeeId, Guid actingEmployeeId, SetEmployeeActiveStatusRequest request, CancellationToken cancellationToken = default);

    Task<Result<EmployeeSummaryDto>> SetManagerAsync(
        Guid employeeId, SetEmployeeManagerRequest request, CancellationToken cancellationToken = default);

    Task<Result<EmployeeSummaryDto>> SetRoleAsync(
        Guid employeeId, Guid actingEmployeeId, SetEmployeeRoleRequest request, CancellationToken cancellationToken = default);
}
