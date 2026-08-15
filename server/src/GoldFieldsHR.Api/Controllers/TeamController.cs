using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TeamController(IEmployeeDirectoryService employeeDirectoryService) : ControllerBase
{
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var reports = await employeeDirectoryService.GetDirectReportsAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(reports);
    }
}
