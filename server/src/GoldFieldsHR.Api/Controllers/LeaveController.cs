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
    [HttpGet("pending/line-manager")]
    public async Task<IActionResult> GetPendingLineManagerApprovals(CancellationToken cancellationToken)
    {
        var requests = await leaveService.GetPendingLineManagerApprovalsAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(requests);
    }

    [Authorize(Roles = "HR")]
    [HttpGet("pending/hr")]
    public async Task<IActionResult> GetPendingHRApprovals(CancellationToken cancellationToken)
    {
        var requests = await leaveService.GetPendingHRApprovalsAsync(cancellationToken);
        return Ok(requests);
    }

    [Authorize(Roles = "LineManager")]
    [HttpPost("{id:guid}/line-manager-review")]
    public async Task<IActionResult> LineManagerReview(Guid id, ReviewLeaveRequest review, CancellationToken cancellationToken)
    {
        var result = await leaveService.LineManagerReviewAsync(id, User.GetEmployeeId(), review, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "HR")]
    [HttpPost("{id:guid}/hr-review")]
    public async Task<IActionResult> HRReview(Guid id, ReviewLeaveRequest review, CancellationToken cancellationToken)
    {
        var result = await leaveService.HRReviewAsync(id, User.GetEmployeeId(), review, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:guid}/signed-document")]
    public async Task<IActionResult> GetSignedDocument(Guid id, CancellationToken cancellationToken)
    {
        var result = await leaveService.GenerateSignedDocumentAsync(id, User.GetEmployeeId(), cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { error = result.Error });
        }

        return File(result.Value!, "application/pdf", $"leave-request-{id}.pdf");
    }
}
