using tracking_management_api.Dtos;

namespace tracking_management_api.Services.SafetyIncidents;

public interface ISafetyIncidentNotifier
{
    Task<Guid> NotifyAsync(SafetyIncidentDto incident, CancellationToken cancellationToken = default);
}
