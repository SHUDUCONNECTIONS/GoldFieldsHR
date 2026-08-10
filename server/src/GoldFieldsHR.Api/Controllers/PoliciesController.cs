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
    public async Task<IActionResult> Acknowledge(Guid id, CancellationToken cancellationToken)
    {
        var result = await policyService.AcknowledgeAsync(id, User.GetEmployeeId(), cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "HR")]
    [HttpGet("{id:guid}/acknowledgments")]
    public async Task<IActionResult> GetAcknowledgments(Guid id, CancellationToken cancellationToken)
    {
        var acknowledgments = await policyService.GetAcknowledgmentsAsync(id, cancellationToken);
        return Ok(acknowledgments);
    }
}
