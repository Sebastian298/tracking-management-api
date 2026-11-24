using NSubstitute;
using tracking_management_api.Mappers;
using tracking_management_api.Models;
using tracking_management_api.Repositories.SafetyIncidents;
using tracking_management_api.Services.SafetyIncidents;

namespace tracking_management_api.Tests;

public sealed class SafetyIncidentServiceTests
{
    [Fact]
    public async Task RegisterAsync_MapsPersistsAndReturnsDto()
    {
        var repository = Substitute.For<ISafetyIncidentRepository>();
        var mapper = new SafetyIncidentMapper();
        repository
            .AddAsync(Arg.Any<SafetyIncident>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var incident = call.Arg<SafetyIncident>();
                return incident with { IncidentId = "INC00000171" };
            });
        var service = new SafetyIncidentService(repository, mapper);
        var request = SafetyIncidentFixtures.CreateRequest();

        var dto = await service.RegisterAsync(request);

        Assert.Equal("INC00000171", dto.IncidentId);
        Assert.Equal(request.TripId, dto.TripId);
        Assert.Equal(150.75m, dto.ClaimAmount);
        await repository.Received(1).AddAsync(Arg.Any<SafetyIncident>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        var service = new SafetyIncidentService(
            Substitute.For<ISafetyIncidentRepository>(),
            new SafetyIncidentMapper());

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.RegisterAsync(null!));
    }
}
