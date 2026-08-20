using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.Boards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BoardsController(IBoardService boardService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateBoardRequest request, CancellationToken cancellationToken)
    {
        var result = await boardService.CreateAsync(User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var boards = await boardService.GetMineAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(boards);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await boardService.GetByIdAsync(id, User.GetEmployeeId(), cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateBoardRequest request, CancellationToken cancellationToken)
    {
        var result = await boardService.UpdateAsync(id, User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddMember(Guid id, AddBoardMemberRequest request, CancellationToken cancellationToken)
    {
        var result = await boardService.AddMemberAsync(id, User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{id:guid}/members/{employeeId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid employeeId, CancellationToken cancellationToken)
    {
        var result = await boardService.RemoveMemberAsync(id, User.GetEmployeeId(), employeeId, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}
