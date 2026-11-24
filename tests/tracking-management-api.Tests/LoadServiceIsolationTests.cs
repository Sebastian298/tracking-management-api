using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using tracking_management_api.Mappers;
using tracking_management_api.Models;
using tracking_management_api.Options.Loads;
using tracking_management_api.Repositories.Loads;
using tracking_management_api.Services.Loads;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace tracking_management_api.Tests;

public sealed class LoadServiceIsolationTests
{
    [Fact]
    public async Task DirectLoadService_DoesNotReadMemoryCacheFilledByCachedService()
    {
        var repository = Substitute.For<ILoadRepository>();
        repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(
            new List<Load> { LoadFixtures.CreateLoad("LOAD00000001") });
        var mapper = new LoadMapper();
        var loadService = new LoadService(repository, mapper);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var cachedService = new MemoryCachedLoadService(
            loadService,
            cache,
            OptionsFactory.Create(new LoadsCacheOptions { AbsoluteExpirationSeconds = 60 }),
            NullLogger<MemoryCachedLoadService>.Instance);

        await cachedService.GetAllAsync();
        await loadService.GetAllAsync();
        await loadService.GetAllAsync();

        await repository.Received(3).GetAllAsync(Arg.Any<CancellationToken>());
        Assert.True(cache.TryGetValue(LoadsCacheOptions.AllLoadsCacheKey, out _));
    }
}
