using GoldFieldsHR.Application.Sites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SitesController(ISiteService siteService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var sites = await siteService.GetActiveAsync(cancellationToken);
        return Ok(sites);
    }

    [Authorize(Roles = "HR,Executive")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var sites = await siteService.GetAllAsync(cancellationToken);
        return Ok(sites);
    }

    [Authorize(Roles = "HR")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateSiteRequest request, CancellationToken cancellationToken)
    {
        var result = await siteService.CreateAsync(request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "HR")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateSiteRequest request, CancellationToken cancellationToken)
    {
        var result = await siteService.UpdateAsync(id, request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "HR")]
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid id, SetSiteActiveStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await siteService.SetActiveStatusAsync(id, request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}
