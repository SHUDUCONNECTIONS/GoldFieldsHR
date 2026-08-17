using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Kpi;

public interface IKpiService
{
    Task<IReadOnlyList<KpiTemplateSummaryDto>> GetTemplatesAsync(CancellationToken cancellationToken = default);

    Task<Result<KpiTemplateDetailDto>> GetTemplateByIdAsync(Guid templateId, CancellationToken cancellationToken = default);

    Task<Result<KpiTemplateDetailDto>> CreateTemplateAsync(
        CreateKpiTemplateRequest request, CancellationToken cancellationToken = default);

    Task<Result<KpiTemplateDetailDto>> UpdateTemplateAsync(
        Guid templateId, CreateKpiTemplateRequest request, CancellationToken cancellationToken = default);

    Task<Result<bool>> DeactivateTemplateAsync(Guid templateId, CancellationToken cancellationToken = default);

    Task<Result<KpiAppraisalDetailDto>> CreateAppraisalAsync(
        Guid createdByEmployeeId, CreateKpiAppraisalRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KpiAppraisalSummaryDto>> GetMyAppraisalsAsync(
        Guid employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KpiAppraisalSummaryDto>> GetAppraisalsIManageAsync(
        Guid managerEmployeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KpiAppraisalSummaryDto>> GetAllAppraisalsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KpiAppraisalSummaryDto>> GetPendingMySignOffAsync(
        Guid employeeId, CancellationToken cancellationToken = default);

    Task<Result<KpiAppraisalDetailDto>> GetAppraisalByIdAsync(
        Guid appraisalId, Guid requesterId, CancellationToken cancellationToken = default);

    Task<Result<KpiAppraisalDetailDto>> SubmitCheckpointScoresAsync(
        Guid appraisalId, Guid submitterId, SubmitCheckpointScoresRequest request, CancellationToken cancellationToken = default);

    Task<Result<KpiAppraisalDetailDto>> SetItemFlagsAsync(
        Guid appraisalId, Guid submitterId, SetItemFlagsRequest request, CancellationToken cancellationToken = default);

    Task<Result<KpiAppraisalDetailDto>> SignAsBlastingOfficerAsync(
        Guid appraisalId, Guid signerId, SignKpiAppraisalRequest request, CancellationToken cancellationToken = default);

    Task<Result<KpiAppraisalDetailDto>> SignAsBlastingEngineerAsync(
        Guid appraisalId, Guid signerId, SignKpiAppraisalRequest request, CancellationToken cancellationToken = default);

    Task<Result<byte[]>> GenerateAppraisalPdfAsync(
        Guid appraisalId, Guid requesterId, CancellationToken cancellationToken = default);
}
