using tracking_management_api.Dtos;
using tracking_management_api.Models;

namespace tracking_management_api.Mappers;

public interface ILoadMapper
{
    LoadDto ToDto(Load load);

    IReadOnlyList<LoadDto> ToDtoList(IEnumerable<Load> loads);
}
