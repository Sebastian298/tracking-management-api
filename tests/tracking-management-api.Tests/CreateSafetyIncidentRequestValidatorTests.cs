using tracking_management_api.Validators;

namespace tracking_management_api.Tests;

public sealed class CreateSafetyIncidentRequestValidatorTests
{
    private readonly CreateSafetyIncidentRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestIsValid_Succeeds()
    {
        var result = _validator.Validate(SafetyIncidentFixtures.CreateRequest());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("TRIP1")]
    [InlineData("TRIP0003607")]
    [InlineData("trip00036079")]
    [InlineData("")]
    public void Validate_WhenTripIdIsInvalid_Fails(string tripId)
    {
        var result = _validator.Validate(SafetyIncidentFixtures.CreateRequest(tripId: tripId));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "TripId");
    }

    [Theory]
    [InlineData("TRK1")]
    [InlineData("TRK0006")]
    [InlineData("trk00006")]
    public void Validate_WhenTruckIdIsInvalid_Fails(string truckId)
    {
        var result = _validator.Validate(SafetyIncidentFixtures.CreateRequest(truckId: truckId));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "TruckId");
    }

    [Theory]
    [InlineData("DRV1")]
    [InlineData("DRV0006")]
    [InlineData("drv00006")]
    public void Validate_WhenDriverIdIsInvalid_Fails(string driverId)
    {
        var result = _validator.Validate(SafetyIncidentFixtures.CreateRequest(driverId: driverId));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "DriverId");
    }

    [Fact]
    public void Validate_WhenIncidentTypeIsUnknown_Fails()
    {
        var result = _validator.Validate(SafetyIncidentFixtures.CreateRequest(incidentType: "Weather"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "IncidentType");
    }

    [Fact]
    public void Validate_WhenIncidentDateIsInTheFuture_Fails()
    {
        var result = _validator.Validate(
            SafetyIncidentFixtures.CreateRequest(incidentDate: DateTime.UtcNow.AddDays(2)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "IncidentDate");
    }

    [Fact]
    public void Validate_WhenVehicleDamageCostIsNegative_Fails()
    {
        var result = _validator.Validate(SafetyIncidentFixtures.CreateRequest(vehicleDamageCost: -1m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "VehicleDamageCost");
    }

    [Fact]
    public void Validate_WhenCargoDamageCostIsNegative_Fails()
    {
        var result = _validator.Validate(SafetyIncidentFixtures.CreateRequest(cargoDamageCost: -0.01m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "CargoDamageCost");
    }

    [Fact]
    public void Validate_WhenLocationStateIsNotTwoLetters_Fails()
    {
        var result = _validator.Validate(SafetyIncidentFixtures.CreateRequest(locationState: "pa"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "LocationState");
    }
}
