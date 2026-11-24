using tracking_management_api.Dtos;
using tracking_management_api.Models;

namespace tracking_management_api.Mappers;

public sealed class LoadMapper : ILoadMapper
{
    public LoadDto ToDto(Load load)
    {
        ArgumentNullException.ThrowIfNull(load);

        return new LoadDto(
            load.LoadId,
            load.CustomerId,
            load.RouteId,
            load.LoadDate,
            load.LoadType,
            load.WeightLbs,
            load.Pieces,
            load.Revenue,
            load.FuelSurcharge,
            load.AccessorialCharges,
            load.LoadStatus,
            load.BookingType);
    }

    public IReadOnlyList<LoadDto> ToDtoList(IEnumerable<Load> loads)
    {
        ArgumentNullException.ThrowIfNull(loads);

        return loads.Select(ToDto).ToList();
    }
}
