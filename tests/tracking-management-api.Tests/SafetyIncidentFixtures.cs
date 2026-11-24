using tracking_management_api.Dtos;
using tracking_management_api.Models;

namespace tracking_management_api.Tests;

internal static class SafetyIncidentFixtures
{
    public static CreateSafetyIncidentRequest CreateRequest(
        string tripId = "TRIP00036079",
        string truckId = "TRK00006",
        string driverId = "DRV00006",
        DateTime? incidentDate = null,
        string incidentType = "Moving Violation",
        string locationCity = "Columbus",
        string locationState = "PA",
        bool atFaultFlag = true,
        bool injuryFlag = false,
        decimal vehicleDamageCost = 100.50m,
        decimal cargoDamageCost = 50.25m,
        bool preventableFlag = true,
        string description = "Severe incident involving equipment")
    {
        return new CreateSafetyIncidentRequest(
            tripId,
            truckId,
            driverId,
            incidentDate ?? new DateTime(2023, 4, 9, 14, 0, 0),
            incidentType,
            locationCity,
            locationState,
            atFaultFlag,
            injuryFlag,
            vehicleDamageCost,
            cargoDamageCost,
            preventableFlag,
            description);
    }

    public static SafetyIncident CreateIncident(string incidentId = "INC00000001")
    {
        var request = CreateRequest();
        return new SafetyIncident(
            incidentId,
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

    public static SafetyIncidentDto CreateDto(string incidentId = "INC00000001")
    {
        var incident = CreateIncident(incidentId);
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
}
