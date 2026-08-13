using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.LegalAppointments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LegalAppointmentsController(ILegalAppointmentService legalAppointmentService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit(SubmitLegalAppointmentRequest request, CancellationToken cancellationToken)
    {
        var result = await legalAppointmentService.SubmitAsync(User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var appointments = await legalAppointmentService.GetMyAppointmentsAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(appointments);
    }

    [Authorize(Roles = "SafetyOfficer")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        var appointments = await legalAppointmentService.GetPendingApprovalsAsync(cancellationToken);
        return Ok(appointments);
    }

    [Authorize(Roles = "SafetyOfficer")]
    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var appointments = await legalAppointmentService.GetActiveAppointmentsAsync(cancellationToken);
        return Ok(appointments);
    }

    [Authorize(Roles = "SafetyOfficer")]
    [HttpPost("{id:guid}/review")]
    public async Task<IActionResult> Review(Guid id, ReviewLegalAppointmentRequest review, CancellationToken cancellationToken)
    {
        var result = await legalAppointmentService.ReviewAsync(id, User.GetEmployeeId(), review, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "SafetyOfficer")]
    [HttpPost("{id:guid}/revoke")]
    public async Task<IActionResult> Revoke(Guid id, RevokeLegalAppointmentRequest request, CancellationToken cancellationToken)
    {
        var result = await legalAppointmentService.RevokeAsync(id, request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}
