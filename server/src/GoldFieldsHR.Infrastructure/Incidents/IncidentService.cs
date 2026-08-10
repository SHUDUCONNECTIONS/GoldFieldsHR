using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Incidents;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Incidents;

public class IncidentService(ApplicationDbContext dbContext) : IIncidentService
{
    public async Task<Result<IncidentReportDto>> SubmitAsync(
        Guid employeeId, SubmitIncidentReportRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees.FindAsync([employeeId], cancellationToken);
        if (employee is null)
        {
            return Result<IncidentReportDto>.Failure("Employee profile not found.");
        }

        var entity = new IncidentReport
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            Title = request.Title,
            Description = request.Description,
            Severity = request.Severity,
            Location = request.Location,
            OccurredAtUtc = request.OccurredAtUtc,
        };

        dbContext.IncidentReports.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<IncidentReportDto>.Success(ToDto(entity, employee.FullName));
    }

    public async Task<IReadOnlyList<IncidentReportDto>> GetMyReportsAsync(
        Guid employeeId, CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.IncidentReports
            .Include(r => r.Employee)
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(r => ToDto(r, r.Employee!.FullName)).ToList();
    }

    public async Task<IReadOnlyList<IncidentReportDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.IncidentReports
            .Include(r => r.Employee)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(r => ToDto(r, r.Employee!.FullName)).ToList();
    }

    public async Task<Result<IncidentReportDto>> UpdateStatusAsync(
        Guid incidentId, Guid reviewerId, UpdateIncidentStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.IncidentReports
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.Id == incidentId, cancellationToken);

        if (entity is null)
        {
            return Result<IncidentReportDto>.Failure("Incident report not found.");
        }

        if (request.Status <= entity.Status)
        {
            return Result<IncidentReportDto>.Failure(
                $"Status can only move forward. Current status is {entity.Status}.");
        }

        entity.Status = request.Status;
        entity.ReviewerId = reviewerId;
        entity.ReviewedAtUtc = DateTime.UtcNow;
        entity.ReviewNotes = request.ReviewNotes;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<IncidentReportDto>.Success(ToDto(entity, entity.Employee!.FullName));
    }

    private static IncidentReportDto ToDto(IncidentReport entity, string employeeName) => new(
        entity.Id,
        entity.EmployeeId,
        employeeName,
        entity.Title,
        entity.Description,
        entity.Severity,
        entity.Location,
        entity.OccurredAtUtc,
        entity.Status,
        entity.CreatedAtUtc,
        entity.ReviewedAtUtc,
        entity.ReviewNotes);
}
