using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.Permits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PermitsController(IPermitService permitService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit(SubmitPermitRequest request, CancellationToken cancellationToken)
    {
        var result = await permitService.SubmitAsync(User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var permits = await permitService.GetMyPermitsAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(permits);
    }

    [Authorize(Roles = "SafetyOfficer")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        var permits = await permitService.GetPendingApprovalsAsync(cancellationToken);
        return Ok(permits);
    }

    [Authorize(Roles = "SafetyOfficer")]
    [HttpGet("open")]
    public async Task<IActionResult> GetOpen(CancellationToken cancellationToken)
    {
        var permits = await permitService.GetOpenPermitsAsync(cancellationToken);
        return Ok(permits);
    }

    [Authorize(Roles = "SafetyOfficer")]
    [HttpPost("{id:guid}/review")]
    public async Task<IActionResult> Review(Guid id, ReviewPermitRequest review, CancellationToken cancellationToken)
    {
        var result = await permitService.ReviewAsync(id, User.GetEmployeeId(), review, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "SafetyOfficer")]
    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, ClosePermitRequest request, CancellationToken cancellationToken)
    {
        var result = await permitService.CloseAsync(id, request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}
