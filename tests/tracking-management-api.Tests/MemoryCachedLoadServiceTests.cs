using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using tracking_management_api.Dtos;
using tracking_management_api.Mappers;
using tracking_management_api.Models;
using tracking_management_api.Options.Loads;
using tracking_management_api.Services.Loads;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace tracking_management_api.Tests;

public sealed class MemoryCachedLoadServiceTests
{
    [Fact]
    public async Task GetAllAsync_OnMiss_ReadsFromInnerServiceAndCachesTheResult()
    {
        var inner = Substitute.For<ILoadService>();
        var dtos = new List<LoadDto>
        {
            ToDto(LoadFixtures.CreateLoad("LOAD00000001"))
        };
        inner.GetAllAsync(Arg.Any<CancellationToken>()).Returns(dtos);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var service = CreateService(inner, cache);

        var first = await service.GetAllAsync();
        var second = await service.GetAllAsync();

        Assert.False(first.FromCache);
        Assert.True(second.FromCache);
        Assert.Same(first.Loads, second.Loads);
        Assert.Equal("LOAD00000001", first.Loads[0].LoadId);
        await inner.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        Assert.Equal(1, service.Misses);
        Assert.Equal(1, service.Hits);
    }

    [Fact]
    public async Task GetAllAsync_AfterKeyIsRemoved_ReadsFromInnerServiceAgain()
    {
        var inner = Substitute.For<ILoadService>();
        inner.GetAllAsync(Arg.Any<CancellationToken>()).Returns(
            new List<LoadDto> { ToDto(LoadFixtures.CreateLoad("LOAD00000001")) });
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var service = CreateService(inner, cache);

        await service.GetAllAsync();
        cache.Remove(LoadsCacheOptions.AllLoadsCacheKey);
        await service.GetAllAsync();

        await inner.Received(2).GetAllAsync(Arg.Any<CancellationToken>());
        Assert.Equal(2, service.Misses);
    }

    [Fact]
    public async Task GetAllAsync_WhenManyCallersMissTogether_ReadsTheSourceOnce()
    {
        var inner = Substitute.For<ILoadService>();
        inner.GetAllAsync(Arg.Any<CancellationToken>()).Returns(async call =>
        {
            await Task.Delay(50);
            return (IReadOnlyList<LoadDto>)new List<LoadDto>
            {
                ToDto(LoadFixtures.CreateLoad("LOAD00000001"))
            };
        });
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var service = CreateService(inner, cache);

        var results = await Task.WhenAll(
            Enumerable.Range(0, 20).Select(_ => service.GetAllAsync()));

        await inner.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        Assert.Equal(1, service.Misses);
        Assert.Equal(19, service.Hits);
        Assert.All(results, result => Assert.Equal("LOAD00000001", result.Loads[0].LoadId));
        Assert.Equal(1, results.Count(result => !result.FromCache));
        Assert.Equal(19, results.Count(result => result.FromCache));
    }

    private static MemoryCachedLoadService CreateService(ILoadService inner, IMemoryCache cache)
    {
        IOptions<LoadsCacheOptions> options = OptionsFactory.Create(
            new LoadsCacheOptions { AbsoluteExpirationSeconds = 60 });
        return new MemoryCachedLoadService(inner, cache, options, NullLogger<MemoryCachedLoadService>.Instance);
    }

    private static LoadDto ToDto(Load load)
    {
        return new LoadMapper().ToDto(load);
    }
}
