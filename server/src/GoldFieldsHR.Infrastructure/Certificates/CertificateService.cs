using GoldFieldsHR.Application.Certificates;
using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Certificates;

public class CertificateService(ApplicationDbContext dbContext) : ICertificateService
{
    private const int DueSoonThresholdDays = 30;

    public async Task<Result<CertificateDto>> IssueAsync(
        Guid issuerEmployeeId, IssueCertificateRequest request, CancellationToken cancellationToken = default)
    {
        var issuer = await dbContext.Employees.FindAsync([issuerEmployeeId], cancellationToken);
        if (issuer is null)
        {
            return Result<CertificateDto>.Failure("Employee profile not found.");
        }

        var recipient = await dbContext.Employees
            .FirstOrDefaultAsync(e => e.EmployeeNumber == request.EmployeeNumber, cancellationToken);
        if (recipient is null)
        {
            return Result<CertificateDto>.Failure($"No employee found with number '{request.EmployeeNumber}'.");
        }

        if (request.ExpiryDate < request.IssuedDate)
        {
            return Result<CertificateDto>.Failure("Expiry date cannot be before the issued date.");
        }

        var entity = new Certificate
        {
            Id = Guid.NewGuid(),
            EmployeeId = recipient.Id,
            Title = request.Title,
            IssuedDate = request.IssuedDate,
            ExpiryDate = request.ExpiryDate,
            Notes = request.Notes,
            IssuedByEmployeeId = issuerEmployeeId,
        };

        dbContext.Certificates.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<CertificateDto>.Success(ToDto(entity, recipient.FullName, issuer.FullName));
    }

    public async Task<IReadOnlyList<CertificateDto>> GetMyCertificatesAsync(
        Guid employeeId, CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.Certificates
            .Include(c => c.Employee)
            .Include(c => c.IssuedByEmployee)
            .Where(c => c.EmployeeId == employeeId)
            .OrderBy(c => c.ExpiryDate)
            .ToListAsync(cancellationToken);

        return entities.Select(c => ToDto(c, c.Employee!.FullName, c.IssuedByEmployee!.FullName)).ToList();
    }

    public async Task<IReadOnlyList<CertificateDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.Certificates
            .Include(c => c.Employee)
            .Include(c => c.IssuedByEmployee)
            .OrderBy(c => c.ExpiryDate)
            .ToListAsync(cancellationToken);

        return entities.Select(c => ToDto(c, c.Employee!.FullName, c.IssuedByEmployee!.FullName)).ToList();
    }

    private static CertificateDto ToDto(Certificate entity, string employeeName, string issuedByName)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var status = entity.ExpiryDate < today
            ? CertificateStatus.Expired
            : entity.ExpiryDate <= today.AddDays(DueSoonThresholdDays)
                ? CertificateStatus.DueSoon
                : CertificateStatus.Valid;

        return new CertificateDto(
            entity.Id,
            entity.EmployeeId,
            employeeName,
            entity.Title,
            entity.IssuedDate,
            entity.ExpiryDate,
            status,
            entity.Notes,
            issuedByName);
    }
}
