using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Announcements;

public interface IAnnouncementService
{
    Task<Result<AnnouncementDto>> CreateAsync(
        Guid posterEmployeeId, CreateAnnouncementRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnnouncementDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
