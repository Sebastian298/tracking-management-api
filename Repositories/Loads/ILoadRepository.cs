using tracking_management_api.Models;

namespace tracking_management_api.Repositories.Loads;

public interface ILoadRepository
{
    Task<IReadOnlyList<Load>> GetAllAsync(CancellationToken cancellationToken = default);
}
