using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.Timesheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TimesheetController(ITimesheetService timesheetService, IClockingReportParserService clockingReportParserService) : ControllerBase
{
    private const long MaxClockingReportBytes = 20 * 1024 * 1024;


    [HttpPost("clock-in")]
    public async Task<IActionResult> ClockIn(CancellationToken cancellationToken)
    {
        var result = await timesheetService.ClockInAsync(User.GetEmployeeId(), cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPost("clock-out")]
    public async Task<IActionResult> ClockOut(CancellationToken cancellationToken)
    {
        var result = await timesheetService.ClockOutAsync(User.GetEmployeeId(), cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetToday(CancellationToken cancellationToken)
    {
        var entry = await timesheetService.GetOpenEntryAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(entry);
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory(CancellationToken cancellationToken)
    {
        var history = await timesheetService.GetHistoryAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(history);
    }

    [HttpPost("corrections")]
    public async Task<IActionResult> SubmitCorrection(SubmitTimesheetCorrectionRequest request, CancellationToken cancellationToken)
    {
        var result = await timesheetService.SubmitCorrectionAsync(User.GetEmployeeId(), request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("corrections/mine")]
    public async Task<IActionResult> GetMyCorrections(CancellationToken cancellationToken)
    {
        var corrections = await timesheetService.GetMyCorrectionsAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(corrections);
    }

    [Authorize(Roles = "LineManager")]
    [HttpGet("corrections/pending")]
    public async Task<IActionResult> GetPendingCorrections(CancellationToken cancellationToken)
    {
        var corrections = await timesheetService.GetPendingCorrectionApprovalsAsync(User.GetEmployeeId(), cancellationToken);
        return Ok(corrections);
    }

    [Authorize(Roles = "LineManager")]
    [HttpPost("corrections/{id:guid}/review")]
    public async Task<IActionResult> ReviewCorrection(Guid id, ReviewTimesheetCorrectionRequest review, CancellationToken cancellationToken)
    {
        var result = await timesheetService.ReviewCorrectionAsync(id, User.GetEmployeeId(), review, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "HR")]
    [HttpPost("clocking-report-parser")]
    [RequestSizeLimit(MaxClockingReportBytes)]
    public async Task<IActionResult> ParseClockingReport(
        IFormFile file,
        [FromForm] string workDays,
        [FromForm] double hoursPerDay,
        [FromForm] bool rotating,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest(new { error = "The uploaded file is empty." });
        }

        await using var stream = file.OpenReadStream();
        var result = await clockingReportParserService.ParseAsync(
            stream, file.FileName, workDays, hoursPerDay, rotating, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}
