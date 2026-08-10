using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.Medical;

public record RecordMedicalExaminationRequest(
    string EmployeeNumber,
    DateOnly ExamDate,
    DateOnly ExpiryDate,
    FitnessStatus Status,
    string? Restrictions,
    string? Notes);

public record MedicalExaminationDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    DateOnly ExamDate,
    DateOnly ExpiryDate,
    FitnessStatus Status,
    string? Restrictions,
    string? Notes,
    string ExaminedByName);
