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

    [Authorize(Roles = "HR,Executive")]
    [HttpGet("org/summary")]
    public async Task<IActionResult> GetOrgSummary([FromQuery] Guid? siteId, CancellationToken cancellationToken)
    {
        var summary = await boardPerformanceService.GetOrgSummaryAsync(siteId, cancellationToken);
        return Ok(summary);
    }

    [Authorize(Roles = "HR,Executive")]
    [HttpGet("completed-boards")]
    public async Task<IActionResult> GetCompletedBoards([FromQuery] Guid? siteId, CancellationToken cancellationToken)
    {
        var boards = await boardPerformanceService.GetCompletedBoardsAsync(siteId, cancellationToken);
        return Ok(boards);
    }

    [Authorize(Roles = "HR,Executive")]
    [HttpGet("employee/{employeeId:guid}/pdf")]
    public async Task<IActionResult> GetEmployeePdf(Guid employeeId, CancellationToken cancellationToken)
    {
        var result = await boardPerformanceService.GenerateEmployeePerformancePdfAsync(employeeId, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { error = result.Error });
        }

        return File(result.Value!, "application/pdf", $"employee-performance-{employeeId}.pdf");
    }
}
