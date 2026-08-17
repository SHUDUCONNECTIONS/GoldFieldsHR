using GoldFieldsHR.Application.Account;
using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Infrastructure.Identity;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Account;

public class AccountService(UserManager<AppUser> userManager, ApplicationDbContext dbContext) : IAccountService
{
    public async Task<ProfileDto?> GetProfileAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees
            .Include(e => e.Site)
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);

        if (employee is null)
        {
            return null;
        }

        var user = await userManager.FindByIdAsync(employee.UserId.ToString());

        return new ProfileDto(
            employee.Id,
            employee.FullName,
            user?.Email ?? string.Empty,
            employee.EmployeeNumber,
            employee.JobTitle,
            employee.Role,
            employee.Site?.Name ?? string.Empty);
    }

    public async Task<Result<bool>> ChangePasswordAsync(
        Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result<bool>.Failure("User not found.");
        }

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        return result.Succeeded
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(string.Join(" ", result.Errors.Select(e => e.Description)));
    }

    public async Task<Result<SignatureDto>> GetSignatureAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees.FindAsync([employeeId], cancellationToken);
        if (employee is null)
        {
            return Result<SignatureDto>.Failure("Employee profile not found.");
        }

        return Result<SignatureDto>.Success(ToDto(employee));
    }

    public async Task<Result<SignatureDto>> SetSignatureAsync(
        Guid employeeId, SetSignatureRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees.FindAsync([employeeId], cancellationToken);
        if (employee is null)
        {
            return Result<SignatureDto>.Failure("Employee profile not found.");
        }

        byte[] signatureBytes;
        try
        {
            signatureBytes = SignatureImageCodec.Decode(request.SignaturePngBase64);
        }
        catch (FormatException)
        {
            return Result<SignatureDto>.Failure("The signature image could not be read.");
        }

        employee.SignatureImageData = signatureBytes;
        employee.SignatureUpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<SignatureDto>.Success(ToDto(employee));
    }

    private static SignatureDto ToDto(Employee employee) => new(
        employee.SignatureImageData is not null,
        employee.SignatureImageData is null ? null : SignatureImageCodec.Encode(employee.SignatureImageData),
        employee.SignatureUpdatedAtUtc);
}
