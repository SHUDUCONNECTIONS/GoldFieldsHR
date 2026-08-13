using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.LegalAppointments;

public interface ILegalAppointmentService
{
    Task<Result<LegalAppointmentDto>> SubmitAsync(
        Guid employeeId, SubmitLegalAppointmentRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LegalAppointmentDto>> GetMyAppointmentsAsync(
        Guid employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LegalAppointmentDto>> GetPendingApprovalsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LegalAppointmentDto>> GetActiveAppointmentsAsync(
        CancellationToken cancellationToken = default);

    Task<Result<LegalAppointmentDto>> ReviewAsync(
        Guid appointmentId, Guid reviewerId, ReviewLegalAppointmentRequest review, CancellationToken cancellationToken = default);

    Task<Result<LegalAppointmentDto>> RevokeAsync(
        Guid appointmentId, RevokeLegalAppointmentRequest request, CancellationToken cancellationToken = default);
}
