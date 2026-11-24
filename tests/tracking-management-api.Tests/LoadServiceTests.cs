using NSubstitute;
using tracking_management_api.Mappers;
using tracking_management_api.Models;
using tracking_management_api.Repositories.Loads;
using tracking_management_api.Services.Loads;

namespace tracking_management_api.Tests;

public sealed class LoadServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtosFromRepository()
    {
        var repository = Substitute.For<ILoadRepository>();
        var mapper = new LoadMapper();
        var loads = new List<Load>
        {
            LoadFixtures.CreateLoad("LOAD00000001"),
            LoadFixtures.CreateLoad("LOAD00000002")
        };
        repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(loads);
        var service = new LoadService(repository, mapper);

        var result = await service.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("LOAD00000001", result[0].LoadId);
        Assert.Equal("LOAD00000002", result[1].LoadId);
        await repository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllAsync_WhenRepositoryReturnsEmpty_ReturnsEmptyList()
    {
        var repository = Substitute.For<ILoadRepository>();
        repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<Load>());
        var service = new LoadService(repository, new LoadMapper());

        var result = await service.GetAllAsync();

        Assert.Empty(result);
    }
}
