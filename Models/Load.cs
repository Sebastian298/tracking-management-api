namespace tracking_management_api.Models;

public sealed record Load(
    string LoadId,
    string CustomerId,
    string RouteId,
    DateOnly LoadDate,
    string LoadType,
    int WeightLbs,
    int Pieces,
    decimal Revenue,
    decimal FuelSurcharge,
    decimal AccessorialCharges,
    string LoadStatus,
    string BookingType);
