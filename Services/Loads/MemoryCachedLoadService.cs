using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using tracking_management_api.Dtos;
using tracking_management_api.Options.Loads;

namespace tracking_management_api.Services.Loads;

public sealed class MemoryCachedLoadService : ICachedLoadService, ILoadCacheStats, IDisposable
{
    private readonly ILoadService _loadService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCachedLoadService> _logger;
    private readonly LoadsCacheOptions _options;
    private readonly SemaphoreSlim _missGate = new(1, 1);
    private long _hits;
    private long _misses;

    public MemoryCachedLoadService(
        ILoadService loadService,
        IMemoryCache cache,
        IOptions<LoadsCacheOptions> options,
        ILogger<MemoryCachedLoadService> logger)
    {
        ArgumentNullException.ThrowIfNull(loadService);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _loadService = loadService;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public long Hits => Interlocked.Read(ref _hits);

    public long Misses => Interlocked.Read(ref _misses);

    public async Task<LoadReadResult> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (TryGetCached(out var cached))
        {
            Interlocked.Increment(ref _hits);
            _logger.LogInformation("Load cache hit for {CacheKey}. Skipping CSV read.", LoadsCacheOptions.AllLoadsCacheKey);
            return new LoadReadResult(cached, FromCache: true);
        }

        await _missGate.WaitAsync(cancellationToken);
        try
        {
            if (TryGetCached(out cached))
            {
                Interlocked.Increment(ref _hits);
                _logger.LogInformation("Load cache hit for {CacheKey}. Skipping CSV read.", LoadsCacheOptions.AllLoadsCacheKey);
                return new LoadReadResult(cached, FromCache: true);
            }

            Interlocked.Increment(ref _misses);
            _logger.LogInformation(
                "Load cache miss for {CacheKey}. Reading from the CSV source.",
                LoadsCacheOptions.AllLoadsCacheKey);

            var loads = await _loadService.GetAllAsync(cancellationToken);
            var entryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.AbsoluteExpirationSeconds)
            };
            _cache.Set(LoadsCacheOptions.AllLoadsCacheKey, loads, entryOptions);
            return new LoadReadResult(loads, FromCache: false);
        }
        finally
        {
            _missGate.Release();
        }
    }

    public void Dispose()
    {
        _missGate.Dispose();
    }

    private bool TryGetCached(out IReadOnlyList<LoadDto> loads)
    {
        if (_cache.TryGetValue(LoadsCacheOptions.AllLoadsCacheKey, out IReadOnlyList<LoadDto>? cached)
            && cached is not null)
        {
            loads = cached;
            return true;
        }

        loads = Array.Empty<LoadDto>();
        return false;
    }
}
