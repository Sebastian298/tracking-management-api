namespace tracking_management_api.Dtos;

public sealed record CreateSafetyIncidentRequest(
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
    bool PreventableFlag,
    string Description);
