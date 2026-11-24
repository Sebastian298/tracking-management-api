namespace tracking_management_api.Services.Loads;

public interface ILoadCacheStats
{
    long Hits { get; }

    long Misses { get; }
}
