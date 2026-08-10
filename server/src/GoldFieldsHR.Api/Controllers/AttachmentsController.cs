using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.Attachments;
using GoldFieldsHR.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AttachmentsController(IAttachmentService attachmentService) : ControllerBase
{
    private const long MaxUploadBytes = 10 * 1024 * 1024;

    [HttpPost("{entityType:int}/{entityId:guid}")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<IActionResult> Upload(
        AttachmentEntityType entityType, Guid entityId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest(new { error = "The uploaded file is empty." });
        }

        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);

        var result = await attachmentService.UploadAsync(
            entityType, entityId, User.GetEmployeeId(),
            new UploadAttachmentRequest(file.FileName, file.ContentType, stream.ToArray()),
            cancellationToken);

        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{entityType:int}/{entityId:guid}")]
    public async Task<IActionResult> GetForEntity(AttachmentEntityType entityType, Guid entityId, CancellationToken cancellationToken)
    {
        var result = await attachmentService.GetForEntityAsync(entityType, entityId, User.GetEmployeeId(), cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var result = await attachmentService.DownloadAsync(id, User.GetEmployeeId(), cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { error = result.Error });
        }

        return File(result.Value!.Content, result.Value.ContentType, result.Value.FileName);
    }
}
