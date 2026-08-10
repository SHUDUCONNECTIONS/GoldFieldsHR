using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.Ppe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PpeController(IPpeService ppeService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit(SubmitPpeRequest request, CancellationToken cancellationToken)
    {
        var result = await ppeService.SubmitAsync(User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var requests = await ppeService.GetMyRequestsAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(requests);
    }

    [Authorize(Roles = "SafetyOfficer")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        var requests = await ppeService.GetPendingApprovalsAsync(cancellationToken);
        return Ok(requests);
    }

    [Authorize(Roles = "SafetyOfficer")]
    [HttpGet("awaiting-issue")]
    public async Task<IActionResult> GetAwaitingIssue(CancellationToken cancellationToken)
    {
        var requests = await ppeService.GetAwaitingIssueAsync(cancellationToken);
        return Ok(requests);
    }

    [Authorize(Roles = "SafetyOfficer")]
    [HttpPost("{id:guid}/review")]
    public async Task<IActionResult> Review(Guid id, ReviewPpeRequest review, CancellationToken cancellationToken)
    {
        var result = await ppeService.ReviewAsync(id, User.GetEmployeeId(), review, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "SafetyOfficer")]
    [HttpPost("{id:guid}/issue")]
    public async Task<IActionResult> Issue(Guid id, CancellationToken cancellationToken)
    {
        var result = await ppeService.MarkIssuedAsync(id, User.GetEmployeeId(), cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}
