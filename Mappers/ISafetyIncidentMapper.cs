using tracking_management_api.Dtos;
using tracking_management_api.Models;

namespace tracking_management_api.Mappers;

public interface ISafetyIncidentMapper
{
    SafetyIncident ToModel(CreateSafetyIncidentRequest request);

    SafetyIncidentDto ToDto(SafetyIncident incident);

    SafetyIncidentRegisteredMessage ToMessage(SafetyIncident incident);

    SafetyIncidentRegisteredMessage ToMessage(SafetyIncidentDto incident);
}
