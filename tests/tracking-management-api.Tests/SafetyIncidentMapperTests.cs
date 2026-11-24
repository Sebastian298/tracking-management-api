using tracking_management_api.Mappers;

namespace tracking_management_api.Tests;

public sealed class SafetyIncidentMapperTests
{
    private readonly SafetyIncidentMapper _mapper = new();

    [Fact]
    public void ToModel_ComputesClaimAmountFromDamageCosts()
    {
        var request = SafetyIncidentFixtures.CreateRequest(
            vehicleDamageCost: 100.50m,
            cargoDamageCost: 50.25m);

        var model = _mapper.ToModel(request);

        Assert.Equal(150.75m, model.ClaimAmount);
        Assert.Equal(string.Empty, model.IncidentId);
        Assert.Equal(request.TripId, model.TripId);
        Assert.Equal(request.IncidentType, model.IncidentType);
    }

    [Fact]
    public void ToDto_MapsAllFields()
    {
        var incident = SafetyIncidentFixtures.CreateIncident("INC00000171");

        var dto = _mapper.ToDto(incident);

        Assert.Equal(incident.IncidentId, dto.IncidentId);
        Assert.Equal(incident.TripId, dto.TripId);
        Assert.Equal(incident.TruckId, dto.TruckId);
        Assert.Equal(incident.DriverId, dto.DriverId);
        Assert.Equal(incident.IncidentDate, dto.IncidentDate);
        Assert.Equal(incident.IncidentType, dto.IncidentType);
        Assert.Equal(incident.LocationCity, dto.LocationCity);
        Assert.Equal(incident.LocationState, dto.LocationState);
        Assert.Equal(incident.AtFaultFlag, dto.AtFaultFlag);
        Assert.Equal(incident.InjuryFlag, dto.InjuryFlag);
        Assert.Equal(incident.VehicleDamageCost, dto.VehicleDamageCost);
        Assert.Equal(incident.CargoDamageCost, dto.CargoDamageCost);
        Assert.Equal(incident.ClaimAmount, dto.ClaimAmount);
        Assert.Equal(incident.PreventableFlag, dto.PreventableFlag);
        Assert.Equal(incident.Description, dto.Description);
    }

    [Fact]
    public void ToMessage_IncludesIncidentAndGeneratedMetadata()
    {
        var before = DateTime.UtcNow;
        var incident = SafetyIncidentFixtures.CreateIncident("INC00000171");

        var message = _mapper.ToMessage(incident);

        Assert.NotEqual(Guid.Empty, message.MessageId);
        Assert.True(message.OccurredAtUtc >= before);
        Assert.True(message.OccurredAtUtc <= DateTime.UtcNow);
        Assert.Equal(incident.IncidentId, message.Incident.IncidentId);
        Assert.Equal(incident.ClaimAmount, message.Incident.ClaimAmount);
    }

    [Fact]
    public void ToModel_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _mapper.ToModel(null!));
    }
}
