using GoldFieldsHR.Application.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GoldFieldsHR.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController(IAuthService authService, IHostEnvironment environment) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : Unauthorized(new { errors = result.Errors });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var token = await authService.RequestPasswordResetAsync(request.Email, cancellationToken);

        // Always respond the same way regardless of whether the email exists, to avoid leaking
        // account existence. No email provider is wired up yet, so in Development the token is
        // returned directly instead of being emailed — a real deployment must swap this for an
        // email/SMTP integration that sends the token as a link and never returns it in the response.
        if (environment.IsDevelopment())
        {
            return Ok(new
            {
                message = "If that email exists, a reset token has been generated.",
                devResetToken = token,
            });
        }

        return Ok(new { message = "If that email exists, a reset token has been generated." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.ResetPasswordAsync(request, cancellationToken);
        return result.Succeeded ? Ok() : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RefreshAsync(request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : Unauthorized(new { errors = result.Errors });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(request, cancellationToken);
        return Ok();
    }
}
