using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.Certificates;

public record IssueCertificateRequest(
    string EmployeeNumber,
    string Title,
    DateOnly IssuedDate,
    DateOnly ExpiryDate,
    string? Notes);

public record CertificateDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    string Title,
    DateOnly IssuedDate,
    DateOnly ExpiryDate,
    CertificateStatus Status,
    string? Notes,
    string IssuedByName);
