using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.Leave;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LeaveController(ILeaveService leaveService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit(SubmitLeaveRequest request, CancellationToken cancellationToken)
    {
        var result = await leaveService.SubmitAsync(User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var requests = await leaveService.GetMyRequestsAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(requests);
    }

    [Authorize(Roles = "LineManager")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        var requests = await leaveService.GetPendingApprovalsAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(requests);
    }

    [Authorize(Roles = "LineManager")]
    [HttpPost("{id:guid}/review")]
    public async Task<IActionResult> Review(Guid id, ReviewLeaveRequest review, CancellationToken cancellationToken)
    {
        var result = await leaveService.ReviewAsync(id, User.GetEmployeeId(), review, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}
