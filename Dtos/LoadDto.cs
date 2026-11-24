namespace tracking_management_api.Dtos;

public sealed record LoadDto(
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
