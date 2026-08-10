using GoldFieldsHR.Api.Common;
using GoldFieldsHR.Application.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldFieldsHR.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AccountController(IAccountService accountService) : ControllerBase
{
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var profile = await accountService.GetProfileAsync(User.GetEmployeeId(), cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await accountService.ChangePasswordAsync(User.GetUserId(), request, cancellationToken);
        return result.Succeeded ? Ok() : BadRequest(new { error = result.Error });
    }
}
