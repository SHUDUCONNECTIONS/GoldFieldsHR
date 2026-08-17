using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PoliciesController(IPolicyService policyService) : ControllerBase
{
    [Authorize(Roles = "HR")]
    [HttpPost]
    public async Task<IActionResult> Create(CreatePolicyRequest request, CancellationToken cancellationToken)
    {
        var result = await policyService.CreateAsync(User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var policies = await policyService.GetAllAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(policies);
    }

    [HttpPost("{id:guid}/acknowledge")]
    public async Task<IActionResult> Acknowledge(Guid id, AcknowledgePolicyRequest request, CancellationToken cancellationToken)
    {
        var result = await policyService.AcknowledgeAsync(id, User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "HR")]
    [HttpGet("{id:guid}/acknowledgments")]
    public async Task<IActionResult> GetAcknowledgments(Guid id, CancellationToken cancellationToken)
    {
        var acknowledgments = await policyService.GetAcknowledgmentsAsync(id, cancellationToken);
        return Ok(acknowledgments);
    }

    [Authorize(Roles = "HR")]
    [HttpGet("{id:guid}/acknowledgments/{employeeId:guid}/attachments/{attachmentId:guid}/signed")]
    public async Task<IActionResult> DownloadSignedAttachment(
        Guid id, Guid employeeId, Guid attachmentId, CancellationToken cancellationToken)
    {
        var result = await policyService.DownloadSignedAttachmentAsync(
            id, employeeId, attachmentId, User.GetEmployeeId(), cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { error = result.Error });
        }

        return File(result.Value!.Content, result.Value.ContentType, result.Value.FileName);
    }
}
