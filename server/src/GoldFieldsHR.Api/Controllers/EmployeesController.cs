using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize(Roles = "HR,Executive")]
[ApiController]
[Route("api/[controller]")]
public class EmployeesController(IEmployeeDirectoryService employeeDirectoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var employees = await employeeDirectoryService.GetAllAsync(cancellationToken);
        return Ok(employees);
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
}
