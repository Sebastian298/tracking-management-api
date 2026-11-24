namespace tracking_management_api.Dtos;

public sealed record SafetyIncidentDto(
    string IncidentId,
    string TripId,
    string TruckId,
    string DriverId,
    DateTime IncidentDate,
    string IncidentType,
    string LocationCity,
    string LocationState,
    bool AtFaultFlag,
    bool InjuryFlag,
    decimal VehicleDamageCost,
    decimal CargoDamageCost,
    decimal ClaimAmount,
    bool PreventableFlag,
    string Description);
