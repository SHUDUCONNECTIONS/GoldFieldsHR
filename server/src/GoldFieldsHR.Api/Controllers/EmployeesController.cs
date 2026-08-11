using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.Employees;
using GoldFieldsHR.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize(Roles = "HR,Executive")]
[ApiController]
[Route("api/[controller]")]
public class EmployeesController(IEmployeeDirectoryService employeeDirectoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string? search,
        [FromQuery] EmployeeRole? role,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] bool? hasRequestedRole = null,
        CancellationToken cancellationToken = default)
    {
        var result = await employeeDirectoryService.GetPagedAsync(
            new EmployeeDirectoryQuery(search, role, isActive, page, pageSize, hasRequestedRole), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "HR")]
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid id, SetEmployeeActiveStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await employeeDirectoryService.SetActiveStatusAsync(id, User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "HR")]
    [HttpPatch("{id:guid}/manager")]
    public async Task<IActionResult> SetManager(Guid id, SetEmployeeManagerRequest request, CancellationToken cancellationToken)
    {
        var result = await employeeDirectoryService.SetManagerAsync(id, request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "HR")]
    [HttpPatch("{id:guid}/role")]
    public async Task<IActionResult> SetRole(Guid id, SetEmployeeRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await employeeDirectoryService.SetRoleAsync(id, User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "HR")]
    [HttpPatch("{id:guid}/requested-role/dismiss")]
    public async Task<IActionResult> DismissRequestedRole(Guid id, CancellationToken cancellationToken)
    {
        var result = await employeeDirectoryService.DismissRequestedRoleAsync(id, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}
