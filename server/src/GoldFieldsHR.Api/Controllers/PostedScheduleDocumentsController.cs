using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.WorkShift;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/schedule-documents")]
public class PostedScheduleDocumentsController(IPostedScheduleDocumentService scheduleDocumentService) : ControllerBase
{
    [Authorize(Roles = "HR")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateScheduleDocumentRequest request, CancellationToken cancellationToken)
    {
        var result = await scheduleDocumentService.CreateAsync(User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var documents = await scheduleDocumentService.GetAllAsync(cancellationToken);
        return Ok(documents);
    }

    [Authorize(Roles = "HR")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await scheduleDocumentService.DeleteAsync(id, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(new { error = result.Error });
    }
}
