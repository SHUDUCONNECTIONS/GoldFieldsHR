using System.Security.Cryptography;
using GoldFieldsHR.Application.Auth;
using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Common.Interfaces;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Identity;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Auth;

public class AuthService(
    UserManager<AppUser> userManager,
    ApplicationDbContext dbContext,
    IJwtTokenGenerator jwtTokenGenerator) : IAuthService
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(14);

    public async Task<AuthResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Role != EmployeeRole.Employee)
        {
            return AuthResult<AuthResponse>.Failure(
                "Self-registration is only available for the Employee role. Ask HR to assign a different role after you sign up.");
        }

        var siteExists = await dbContext.Sites.AnyAsync(s => s.Id == request.SiteId, cancellationToken);
        if (!siteExists)
        {
            return AuthResult<AuthResponse>.Failure("The specified site does not exist.");
        }

        Guid? managerId = null;
        if (!string.IsNullOrWhiteSpace(request.ManagerEmployeeNumber))
        {
            var manager = await dbContext.Employees
                .FirstOrDefaultAsync(e => e.EmployeeNumber == request.ManagerEmployeeNumber, cancellationToken);
            if (manager is null)
            {
                return AuthResult<AuthResponse>.Failure(
                    $"No employee found with manager number '{request.ManagerEmployeeNumber}'.");
            }

            managerId = manager.Id;
        }

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return AuthResult<AuthResponse>.Failure(createResult.Errors.Select(e => e.Description).ToArray());
        }

        await userManager.AddToRoleAsync(user, request.Role.ToString());

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            EmployeeNumber = request.EmployeeNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            JobTitle = request.JobTitle,
            Role = request.Role,
            SiteId = request.SiteId,
            ManagerId = managerId
        };

        user.EmployeeId = employee.Id;

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync(cancellationToken);
        await userManager.UpdateAsync(user);

        var (token, expiresAtUtc) = jwtTokenGenerator.GenerateToken(user.Id, user.Email!, employee);
        var refreshToken = await IssueRefreshTokenAsync(user.Id, cancellationToken);

        return AuthResult<AuthResponse>.Success(
            new AuthResponse(token, expiresAtUtc, employee.Id, employee.FullName, employee.Role, refreshToken));
    }

    public async Task<AuthResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return AuthResult<AuthResponse>.Failure("Invalid email or password.");
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return AuthResult<AuthResponse>.Failure(
                "This account is temporarily locked after repeated failed sign-in attempts. Try again later.");
        }

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            await userManager.AccessFailedAsync(user);
            return AuthResult<AuthResponse>.Failure("Invalid email or password.");
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var employee = await dbContext.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id, cancellationToken);
        if (employee is null)
        {
            return AuthResult<AuthResponse>.Failure("No employee profile is linked to this account.");
        }

        var (token, expiresAtUtc) = jwtTokenGenerator.GenerateToken(user.Id, user.Email!, employee);
        var refreshToken = await IssueRefreshTokenAsync(user.Id, cancellationToken);

        return AuthResult<AuthResponse>.Success(
            new AuthResponse(token, expiresAtUtc, employee.Id, employee.FullName, employee.Role, refreshToken));
    }

    public async Task<string?> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        return await userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<AuthResult<bool>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return AuthResult<bool>.Failure("Invalid email or reset token.");
        }

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        return result.Succeeded
            ? AuthResult<bool>.Success(true)
            : AuthResult<bool>.Failure(result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<AuthResult<AuthResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var existing = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (existing is null || !existing.IsActive)
        {
            return AuthResult<AuthResponse>.Failure("Invalid or expired refresh token.");
        }

        var user = await userManager.FindByIdAsync(existing.UserId.ToString());
        if (user is null)
        {
            return AuthResult<AuthResponse>.Failure("Invalid or expired refresh token.");
        }

        var employee = await dbContext.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id, cancellationToken);
        if (employee is null)
        {
            return AuthResult<AuthResponse>.Failure("No employee profile is linked to this account.");
        }

        // Rotate: revoke the presented token and issue a fresh one, so a stolen refresh token
        // can only be replayed once before this legitimate client's next refresh invalidates it.
        existing.RevokedAtUtc = DateTime.UtcNow;

        var (newAccessToken, expiresAtUtc) = jwtTokenGenerator.GenerateToken(user.Id, user.Email!, employee);
        var newRefreshToken = await IssueRefreshTokenAsync(user.Id, cancellationToken);

        return AuthResult<AuthResponse>.Success(
            new AuthResponse(newAccessToken, expiresAtUtc, employee.Id, employee.FullName, employee.Role, newRefreshToken));
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var existing = await dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);
        if (existing is not null && existing.RevokedAtUtc is null)
        {
            existing.RevokedAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<string> IssueRefreshTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = HashToken(rawToken),
            ExpiresAtUtc = DateTime.UtcNow.Add(RefreshTokenLifetime),
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return rawToken;
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
}
