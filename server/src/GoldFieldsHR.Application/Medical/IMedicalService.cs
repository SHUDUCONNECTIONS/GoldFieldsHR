using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Medical;

public interface IMedicalService
{
    Task<Result<MedicalExaminationDto>> RecordAsync(
        Guid examinerEmployeeId, RecordMedicalExaminationRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MedicalExaminationDto>> GetMyExaminationsAsync(
        Guid employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MedicalExaminationDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
