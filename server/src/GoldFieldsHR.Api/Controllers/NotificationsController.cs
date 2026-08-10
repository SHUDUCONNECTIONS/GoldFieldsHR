using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var notifications = await notificationService.GetMineAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var count = await notificationService.GetUnreadCountAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(new { count });
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var result = await notificationService.MarkAsReadAsync(id, User.GetEmployeeId(), cancellationToken);
        return result.Succeeded ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        await notificationService.MarkAllAsReadAsync(User.GetEmployeeId(), cancellationToken);
        return Ok();
    }
}
