using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.Medical;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MedicalController(IMedicalService medicalService) : ControllerBase
{
    [Authorize(Roles = "Medical")]
    [HttpPost]
    public async Task<IActionResult> Record(RecordMedicalExaminationRequest request, CancellationToken cancellationToken)
    {
        var result = await medicalService.RecordAsync(User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var examinations = await medicalService.GetMyExaminationsAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(examinations);
    }

    [Authorize(Roles = "Medical")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var examinations = await medicalService.GetAllAsync(cancellationToken);
        return Ok(examinations);
    }
}
