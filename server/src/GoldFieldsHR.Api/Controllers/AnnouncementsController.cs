using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.Announcements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AnnouncementsController(IAnnouncementService announcementService) : ControllerBase
{
    [Authorize(Roles = "HR,Executive")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateAnnouncementRequest request, CancellationToken cancellationToken)
    {
        var result = await announcementService.CreateAsync(User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var announcements = await announcementService.GetAllAsync(cancellationToken);
        return Ok(announcements);
    }
}
