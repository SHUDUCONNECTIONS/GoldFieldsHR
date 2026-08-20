using GoldFieldsHR.Application.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

// Deliberately a separate controller from EmployeesController: that controller carries a
// class-level [Authorize(Roles = "HR,Executive")], and ASP.NET Core AND-combines stacked
// [Authorize] attributes rather than letting a method-level one override the class, so a
// plain [Authorize] here couldn't be added as a method on that controller. Any authenticated
// employee needs this lite directory to pick board members/task assignees.
[Authorize]
[ApiController]
[Route("api/employees")]
public class EmployeeDirectoryLiteController(IEmployeeDirectoryService employeeDirectoryService) : ControllerBase
{
    [HttpGet("directory-lite")]
    public async Task<IActionResult> GetDirectoryLite(CancellationToken cancellationToken)
    {
        var employees = await employeeDirectoryService.GetActiveDirectoryLiteAsync(cancellationToken);
        return Ok(employees);
    }
}
