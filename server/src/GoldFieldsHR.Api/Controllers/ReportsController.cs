using GoldFieldsHR.Application.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize(Roles = "HR,SafetyOfficer,Executive")]
[ApiController]
[Route("api/[controller]")]
public class ReportsController(IReportsService reportsService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await reportsService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }
}
