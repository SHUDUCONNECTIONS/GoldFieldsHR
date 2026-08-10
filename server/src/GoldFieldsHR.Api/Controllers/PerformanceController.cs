using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.Performance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PerformanceController(IPerformanceService performanceService) : ControllerBase
{
    [Authorize(Roles = "LineManager")]
    [HttpPost]
    public async Task<IActionResult> Create(CreatePerformanceReviewRequest request, CancellationToken cancellationToken)
    {
        var result = await performanceService.CreateAsync(User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var reviews = await performanceService.GetMyReviewsAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(reviews);
    }

    [Authorize(Roles = "LineManager")]
    [HttpGet("given")]
    public async Task<IActionResult> GetGiven(CancellationToken cancellationToken)
    {
        var reviews = await performanceService.GetGivenByMeAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(reviews);
    }

    [Authorize(Roles = "HR")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var reviews = await performanceService.GetAllAsync(cancellationToken);
        return Ok(reviews);
    }
}
