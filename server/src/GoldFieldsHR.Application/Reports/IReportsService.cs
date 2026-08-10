namespace GoldFieldsHR.Application.Reports;

public interface IReportsService
{
    Task<ReportsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
