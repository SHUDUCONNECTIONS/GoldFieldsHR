using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Incidents;

public interface IIncidentService
{
    Task<Result<IncidentReportDto>> SubmitAsync(
        Guid employeeId, SubmitIncidentReportRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IncidentReportDto>> GetMyReportsAsync(
        Guid employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IncidentReportDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<IncidentReportDto>> UpdateStatusAsync(
        Guid incidentId, Guid reviewerId, UpdateIncidentStatusRequest request, CancellationToken cancellationToken = default);
}
