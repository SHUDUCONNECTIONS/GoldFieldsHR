using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.Boards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

// Not nested under BoardsController/BoardTasksController's api/boards/{boardId} route since
// performance spans every board an employee touches, not one specific board.
[Authorize]
[ApiController]
[Route("api/performance")]
public class BoardPerformanceController(IBoardPerformanceService boardPerformanceService) : ControllerBase
{
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine([FromQuery] PerformanceRange range, CancellationToken cancellationToken)
    {
        var performance = await boardPerformanceService.GetMyPerformanceAsync(User.GetEmployeeId(), range, cancellationToken);
        return Ok(performance);
    }

    [Authorize(Roles = "HR,Executive")]
    [HttpGet("org")]
    public async Task<IActionResult> GetOrg(
        [FromQuery] Guid? siteId, [FromQuery] PerformanceRange range, CancellationToken cancellationToken)
    {
        var performance = await boardPerformanceService.GetOrgPerformanceAsync(siteId, range, cancellationToken);
        return Ok(performance);
    }
}
