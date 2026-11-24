using tracking_management_api.Dtos;
using tracking_management_api.Models;

namespace tracking_management_api.Mappers;

public sealed class SafetyIncidentMapper : ISafetyIncidentMapper
{
    public SafetyIncident ToModel(CreateSafetyIncidentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new SafetyIncident(
            string.Empty,
            request.TripId,
            request.TruckId,
            request.DriverId,
            request.IncidentDate,
            request.IncidentType,
            request.LocationCity,
            request.LocationState,
            request.AtFaultFlag,
            request.InjuryFlag,
            request.VehicleDamageCost,
            request.CargoDamageCost,
            request.VehicleDamageCost + request.CargoDamageCost,
            request.PreventableFlag,
            request.Description);
    }

    public SafetyIncidentDto ToDto(SafetyIncident incident)
    {
        ArgumentNullException.ThrowIfNull(incident);

        return new SafetyIncidentDto(
            incident.IncidentId,
            incident.TripId,
            incident.TruckId,
            incident.DriverId,
            incident.IncidentDate,
            incident.IncidentType,
            incident.LocationCity,
            incident.LocationState,
            incident.AtFaultFlag,
            incident.InjuryFlag,
            incident.VehicleDamageCost,
            incident.CargoDamageCost,
            incident.ClaimAmount,
            incident.PreventableFlag,
            incident.Description);
    }

    public SafetyIncidentRegisteredMessage ToMessage(SafetyIncident incident)
    {
        ArgumentNullException.ThrowIfNull(incident);

        return ToMessage(ToDto(incident));
    }

    public SafetyIncidentRegisteredMessage ToMessage(SafetyIncidentDto incident)
    {
        ArgumentNullException.ThrowIfNull(incident);

        return new SafetyIncidentRegisteredMessage(
            Guid.NewGuid(),
            DateTime.UtcNow,
            incident);
    }
}
