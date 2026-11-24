using tracking_management_api.Dtos;

namespace tracking_management_api.Services.Loads;

public interface ICachedLoadService
{
    Task<LoadReadResult> GetAllAsync(CancellationToken cancellationToken = default);
}
