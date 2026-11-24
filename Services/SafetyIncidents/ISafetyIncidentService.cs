using tracking_management_api.Dtos;

namespace tracking_management_api.Services.SafetyIncidents;

public interface ISafetyIncidentService
{
    Task<SafetyIncidentDto> RegisterAsync(
        CreateSafetyIncidentRequest request,
        CancellationToken cancellationToken = default);
}
