using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Medical;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Medical;

public class MedicalService(ApplicationDbContext dbContext) : IMedicalService
{
    public async Task<Result<MedicalExaminationDto>> RecordAsync(
        Guid examinerEmployeeId, RecordMedicalExaminationRequest request, CancellationToken cancellationToken = default)
    {
        var examiner = await dbContext.Employees.FindAsync([examinerEmployeeId], cancellationToken);
        if (examiner is null)
        {
            return Result<MedicalExaminationDto>.Failure("Employee profile not found.");
        }

        var recipient = await dbContext.Employees
            .FirstOrDefaultAsync(e => e.EmployeeNumber == request.EmployeeNumber, cancellationToken);
        if (recipient is null)
        {
            return Result<MedicalExaminationDto>.Failure($"No employee found with number '{request.EmployeeNumber}'.");
        }

        if (request.ExpiryDate < request.ExamDate)
        {
            return Result<MedicalExaminationDto>.Failure("Expiry date cannot be before the exam date.");
        }

        if (request.Status == FitnessStatus.FitWithRestrictions && string.IsNullOrWhiteSpace(request.Restrictions))
        {
            return Result<MedicalExaminationDto>.Failure("Restrictions are required when status is 'Fit with restrictions'.");
        }

        var entity = new MedicalExamination
        {
            Id = Guid.NewGuid(),
            EmployeeId = recipient.Id,
            ExamDate = request.ExamDate,
            ExpiryDate = request.ExpiryDate,
            Status = request.Status,
            Restrictions = request.Restrictions,
            Notes = request.Notes,
            ExaminedByEmployeeId = examinerEmployeeId,
        };

        dbContext.MedicalExaminations.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<MedicalExaminationDto>.Success(ToDto(entity, recipient.FullName, examiner.FullName));
    }

    public async Task<IReadOnlyList<MedicalExaminationDto>> GetMyExaminationsAsync(
        Guid employeeId, CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.MedicalExaminations
            .Include(m => m.Employee)
            .Include(m => m.ExaminedByEmployee)
            .Where(m => m.EmployeeId == employeeId)
            .OrderByDescending(m => m.ExamDate)
            .ToListAsync(cancellationToken);

        return entities.Select(m => ToDto(m, m.Employee!.FullName, m.ExaminedByEmployee!.FullName)).ToList();
    }

    public async Task<IReadOnlyList<MedicalExaminationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.MedicalExaminations
            .Include(m => m.Employee)
            .Include(m => m.ExaminedByEmployee)
            .OrderByDescending(m => m.ExamDate)
            .ToListAsync(cancellationToken);

        return entities.Select(m => ToDto(m, m.Employee!.FullName, m.ExaminedByEmployee!.FullName)).ToList();
    }

    private static MedicalExaminationDto ToDto(MedicalExamination entity, string employeeName, string examinedByName) =>
        new(
            entity.Id,
            entity.EmployeeId,
            employeeName,
            entity.ExamDate,
            entity.ExpiryDate,
            entity.Status,
            entity.Restrictions,
            entity.Notes,
            examinedByName);
}
