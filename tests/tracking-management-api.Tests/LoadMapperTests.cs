using tracking_management_api.Mappers;
using tracking_management_api.Models;

namespace tracking_management_api.Tests;

public sealed class LoadMapperTests
{
    private readonly LoadMapper _mapper = new();

    [Fact]
    public void ToDto_MapsAllFields()
    {
        var load = LoadFixtures.CreateLoad();

        var dto = _mapper.ToDto(load);

        Assert.Equal(load.LoadId, dto.LoadId);
        Assert.Equal(load.CustomerId, dto.CustomerId);
        Assert.Equal(load.RouteId, dto.RouteId);
        Assert.Equal(load.LoadDate, dto.LoadDate);
        Assert.Equal(load.LoadType, dto.LoadType);
        Assert.Equal(load.WeightLbs, dto.WeightLbs);
        Assert.Equal(load.Pieces, dto.Pieces);
        Assert.Equal(load.Revenue, dto.Revenue);
        Assert.Equal(load.FuelSurcharge, dto.FuelSurcharge);
        Assert.Equal(load.AccessorialCharges, dto.AccessorialCharges);
        Assert.Equal(load.LoadStatus, dto.LoadStatus);
        Assert.Equal(load.BookingType, dto.BookingType);
    }

    [Fact]
    public void ToDtoList_MapsEveryLoad()
    {
        var loads = new[]
        {
            LoadFixtures.CreateLoad("LOAD00000001"),
            LoadFixtures.CreateLoad("LOAD00000002", "CUST00076", "RTE00058")
        };

        var dtos = _mapper.ToDtoList(loads);

        Assert.Equal(2, dtos.Count);
        Assert.Equal("LOAD00000001", dtos[0].LoadId);
        Assert.Equal("LOAD00000002", dtos[1].LoadId);
        Assert.Equal("CUST00076", dtos[1].CustomerId);
    }

    [Fact]
    public void ToDto_WhenLoadIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _mapper.ToDto(null!));
    }
}
