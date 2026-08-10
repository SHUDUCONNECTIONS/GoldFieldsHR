using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.Safety;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SafetyController(ISafetyService safetyService) : ControllerBase
{
    [HttpPost("pre-shift-check")]
    public async Task<IActionResult> Submit(SubmitPreShiftCheckRequest request, CancellationToken cancellationToken)
    {
        var result = await safetyService.SubmitAsync(User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("pre-shift-check/today")]
    public async Task<IActionResult> GetToday(CancellationToken cancellationToken)
    {
        var entry = await safetyService.GetTodayAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(entry);
    }

    [HttpGet("pre-shift-check")]
    public async Task<IActionResult> GetHistory(CancellationToken cancellationToken)
    {
        var history = await safetyService.GetHistoryAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(history);
    }

    [Authorize(Roles = "SafetyOfficer")]
    [HttpGet("hazards/today")]
    public async Task<IActionResult> GetTodaysHazards(CancellationToken cancellationToken)
    {
        var hazards = await safetyService.GetTodaysHazardsAsync(cancellationToken);
        return Ok(hazards);
    }
}
