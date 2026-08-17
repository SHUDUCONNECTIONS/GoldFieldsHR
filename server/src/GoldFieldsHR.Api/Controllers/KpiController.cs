using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.Kpi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class KpiController(IKpiService kpiService) : ControllerBase
{
    [Authorize(Roles = "HR")]
    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates(CancellationToken cancellationToken)
    {
        var templates = await kpiService.GetTemplatesAsync(cancellationToken);
        return Ok(templates);
    }

    [Authorize(Roles = "HR")]
    [HttpGet("templates/{id:guid}")]
    public async Task<IActionResult> GetTemplate(Guid id, CancellationToken cancellationToken)
    {
        var result = await kpiService.GetTemplateByIdAsync(id, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "HR")]
    [HttpPost("templates")]
    public async Task<IActionResult> CreateTemplate(CreateKpiTemplateRequest request, CancellationToken cancellationToken)
    {
        var result = await kpiService.CreateTemplateAsync(request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "HR")]
    [HttpPut("templates/{id:guid}")]
    public async Task<IActionResult> UpdateTemplate(Guid id, CreateKpiTemplateRequest request, CancellationToken cancellationToken)
    {
        var result = await kpiService.UpdateTemplateAsync(id, request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "HR")]
    [HttpPost("templates/{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateTemplate(Guid id, CancellationToken cancellationToken)
    {
        var result = await kpiService.DeactivateTemplateAsync(id, cancellationToken);
        return result.Succeeded ? Ok() : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "HR")]
    [HttpPost("appraisals")]
    public async Task<IActionResult> CreateAppraisal(CreateKpiAppraisalRequest request, CancellationToken cancellationToken)
    {
        var result = await kpiService.CreateAppraisalAsync(User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("appraisals/mine")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var appraisals = await kpiService.GetMyAppraisalsAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(appraisals);
    }

    [Authorize(Roles = "LineManager")]
    [HttpGet("appraisals/managed")]
    public async Task<IActionResult> GetManaged(CancellationToken cancellationToken)
    {
        var appraisals = await kpiService.GetAppraisalsIManageAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(appraisals);
    }

    [Authorize(Roles = "HR")]
    [HttpGet("appraisals")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var appraisals = await kpiService.GetAllAppraisalsAsync(cancellationToken);
        return Ok(appraisals);
    }

    [HttpGet("appraisals/pending-signoff")]
    public async Task<IActionResult> GetPendingSignOff(CancellationToken cancellationToken)
    {
        var appraisals = await kpiService.GetPendingMySignOffAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(appraisals);
    }

    [HttpGet("appraisals/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await kpiService.GetAppraisalByIdAsync(id, User.GetEmployeeId(), cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "LineManager,HR")]
    [HttpPost("appraisals/{id:guid}/scores")]
    public async Task<IActionResult> SubmitCheckpointScores(
        Guid id, SubmitCheckpointScoresRequest request, CancellationToken cancellationToken)
    {
        var result = await kpiService.SubmitCheckpointScoresAsync(id, User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "LineManager,HR")]
    [HttpPost("appraisals/{id:guid}/item-flags")]
    public async Task<IActionResult> SetItemFlags(Guid id, SetItemFlagsRequest request, CancellationToken cancellationToken)
    {
        var result = await kpiService.SetItemFlagsAsync(id, User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPost("appraisals/{id:guid}/sign/blasting-officer")]
    public async Task<IActionResult> SignAsBlastingOfficer(Guid id, SignKpiAppraisalRequest request, CancellationToken cancellationToken)
    {
        var result = await kpiService.SignAsBlastingOfficerAsync(id, User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPost("appraisals/{id:guid}/sign/blasting-engineer")]
    public async Task<IActionResult> SignAsBlastingEngineer(Guid id, SignKpiAppraisalRequest request, CancellationToken cancellationToken)
    {
        var result = await kpiService.SignAsBlastingEngineerAsync(id, User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("appraisals/{id:guid}/pdf")]
    public async Task<IActionResult> GetPdf(Guid id, CancellationToken cancellationToken)
    {
        var result = await kpiService.GenerateAppraisalPdfAsync(id, User.GetEmployeeId(), cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { error = result.Error });
        }

        return File(result.Value!, "application/pdf", $"kpi-appraisal-{id}.pdf");
    }
}
