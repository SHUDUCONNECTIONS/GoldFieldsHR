using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Emergency;

public interface IEmergencyService
{
    Task<Result<EmergencyAlertDto>> TriggerAsync(
        Guid employeeId, TriggerEmergencyAlertRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmergencyAlertDto>> GetMyAlertsAsync(
        Guid employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmergencyAlertDto>> GetActiveAlertsAsync(
        CancellationToken cancellationToken = default);

    Task<Result<EmergencyAlertDto>> ResolveAsync(
        Guid alertId, Guid resolverId, ResolveEmergencyAlertRequest request, CancellationToken cancellationToken = default);
}
