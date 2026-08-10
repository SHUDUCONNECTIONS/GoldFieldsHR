using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.Emergency;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmergencyController(IEmergencyService emergencyService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Trigger(TriggerEmergencyAlertRequest request, CancellationToken cancellationToken)
    {
        var result = await emergencyService.TriggerAsync(User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var alerts = await emergencyService.GetMyAlertsAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(alerts);
    }

    [Authorize(Roles = "Security")]
    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var alerts = await emergencyService.GetActiveAlertsAsync(cancellationToken);
        return Ok(alerts);
    }

    [Authorize(Roles = "Security")]
    [HttpPost("{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid id, ResolveEmergencyAlertRequest request, CancellationToken cancellationToken)
    {
        var result = await emergencyService.ResolveAsync(id, User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}
