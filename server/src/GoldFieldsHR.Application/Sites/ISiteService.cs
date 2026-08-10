using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Sites;

public interface ISiteService
{
    Task<IReadOnlyList<SiteDto>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SiteDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<SiteDto>> CreateAsync(CreateSiteRequest request, CancellationToken cancellationToken = default);

    Task<Result<SiteDto>> UpdateAsync(Guid id, UpdateSiteRequest request, CancellationToken cancellationToken = default);

    Task<Result<SiteDto>> SetActiveStatusAsync(
        Guid id, SetSiteActiveStatusRequest request, CancellationToken cancellationToken = default);
}
