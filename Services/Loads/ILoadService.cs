using tracking_management_api.Dtos;

namespace tracking_management_api.Services.Loads;

public interface ILoadService
{
    Task<IReadOnlyList<LoadDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
