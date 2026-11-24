using tracking_management_api.Models;

namespace tracking_management_api.Repositories.SafetyIncidents;

public interface ISafetyIncidentRepository
{
    Task<SafetyIncident> AddAsync(SafetyIncident incident, CancellationToken cancellationToken = default);
}
