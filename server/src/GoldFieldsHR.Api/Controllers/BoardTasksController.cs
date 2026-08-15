using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Api.Hubs;
using GoldFieldsHR.Application.Boards;
using GoldFieldsHR.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/boards/{boardId:guid}/tasks")]
public class BoardTasksController(IBoardTaskService boardTaskService, IHubContext<BoardHub> boardHub) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(Guid boardId, CreateBoardTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await boardTaskService.CreateAsync(boardId, User.GetEmployeeId(), request, cancellationToken);
        if (result.Succeeded)
        {
            await NotifyBoardUpdatedAsync(boardId, cancellationToken);
        }
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> GetForBoard(
        Guid boardId, [FromQuery] BoardTaskStatus? status, [FromQuery] Guid? assigneeId, CancellationToken cancellationToken)
    {
        var result = await boardTaskService.GetForBoardAsync(boardId, User.GetEmployeeId(), status, assigneeId, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("weekly-summary")]
    public async Task<IActionResult> GetWeeklySummary(
        Guid boardId, [FromQuery] DateOnly? weekStartUtc, CancellationToken cancellationToken)
    {
        var result = await boardTaskService.GetWeeklySummaryAsync(boardId, User.GetEmployeeId(), weekStartUtc, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("weekly-summary/pdf")]
    public async Task<IActionResult> GetWeeklySummaryPdf(
        Guid boardId, [FromQuery] DateOnly? weekStartUtc, CancellationToken cancellationToken)
    {
        var result = await boardTaskService.GenerateWeeklySummaryPdfAsync(boardId, User.GetEmployeeId(), weekStartUtc, cancellationToken);
        return result.Succeeded
            ? File(result.Value!, "application/pdf", $"weekly-summary-{boardId}.pdf")
            : BadRequest(new { error = result.Error });
    }

    [HttpGet("{taskId:guid}")]
    public async Task<IActionResult> GetById(Guid boardId, Guid taskId, CancellationToken cancellationToken)
    {
        var result = await boardTaskService.GetByIdAsync(boardId, taskId, User.GetEmployeeId(), cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPatch("{taskId:guid}")]
    public async Task<IActionResult> Update(
        Guid boardId, Guid taskId, UpdateBoardTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await boardTaskService.UpdateAsync(boardId, taskId, User.GetEmployeeId(), request, cancellationToken);
        if (result.Succeeded)
        {
            await NotifyBoardUpdatedAsync(boardId, cancellationToken);
        }
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{taskId:guid}/status")]
    public async Task<IActionResult> ChangeStatus(
        Guid boardId, Guid taskId, ChangeTaskStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await boardTaskService.ChangeStatusAsync(boardId, taskId, User.GetEmployeeId(), request, cancellationToken);
        if (result.Succeeded)
        {
            await NotifyBoardUpdatedAsync(boardId, cancellationToken);
        }
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> Delete(Guid boardId, Guid taskId, CancellationToken cancellationToken)
    {
        var result = await boardTaskService.DeleteAsync(boardId, taskId, User.GetEmployeeId(), cancellationToken);
        if (result.Succeeded)
        {
            await NotifyBoardUpdatedAsync(boardId, cancellationToken);
        }
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    private Task NotifyBoardUpdatedAsync(Guid boardId, CancellationToken cancellationToken) =>
        boardHub.Clients.Group(BoardHubGroups.For(boardId)).SendAsync("BoardUpdated", boardId, cancellationToken);
}
