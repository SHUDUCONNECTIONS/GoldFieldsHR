using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.WorkShift;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WorkShiftController(IWorkShiftService workShiftService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit(SubmitShiftChangeRequest request, CancellationToken cancellationToken)
    {
        var result = await workShiftService.SubmitAsync(User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var requests = await workShiftService.GetMyRequestsAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(requests);
    }

    [Authorize(Roles = "LineManager")]
    [HttpGet("pending/line-manager")]
    public async Task<IActionResult> GetPendingLineManagerApprovals(CancellationToken cancellationToken)
    {
        var requests = await workShiftService.GetPendingLineManagerApprovalsAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(requests);
    }

    [Authorize(Roles = "HR")]
    [HttpGet("pending/hr")]
    public async Task<IActionResult> GetPendingHRApprovals(CancellationToken cancellationToken)
    {
        var requests = await workShiftService.GetPendingHRApprovalsAsync(cancellationToken);
        return Ok(requests);
    }

    [Authorize(Roles = "LineManager")]
    [HttpPost("{id:guid}/line-manager-review")]
    public async Task<IActionResult> LineManagerReview(Guid id, ReviewShiftChangeRequest review, CancellationToken cancellationToken)
    {
        var result = await workShiftService.LineManagerReviewAsync(id, User.GetEmployeeId(), review, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "HR")]
    [HttpPost("{id:guid}/hr-review")]
    public async Task<IActionResult> HRReview(Guid id, ReviewShiftChangeRequest review, CancellationToken cancellationToken)
    {
        var result = await workShiftService.HRReviewAsync(id, User.GetEmployeeId(), review, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}
