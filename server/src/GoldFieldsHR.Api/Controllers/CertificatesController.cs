using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.Certificates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CertificatesController(ICertificateService certificateService) : ControllerBase
{
    [Authorize(Roles = "HR")]
    [HttpPost]
    public async Task<IActionResult> Issue(IssueCertificateRequest request, CancellationToken cancellationToken)
    {
        var result = await certificateService.IssueAsync(User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var certificates = await certificateService.GetMyCertificatesAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(certificates);
    }

    [Authorize(Roles = "HR")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var certificates = await certificateService.GetAllAsync(cancellationToken);
        return Ok(certificates);
    }
}
