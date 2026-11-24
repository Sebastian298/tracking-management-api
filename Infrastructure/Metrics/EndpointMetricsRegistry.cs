using System.Collections.Concurrent;

namespace tracking_management_api.Infrastructure.Metrics;

public sealed class EndpointMetricsRegistry : IEndpointMetricsRegistry
{
    private readonly ConcurrentDictionary<string, EndpointMetrics> _metrics = new(StringComparer.Ordinal);

    public EndpointMetrics Get(string endpointName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpointName);
        return _metrics.GetOrAdd(endpointName, name => new EndpointMetrics(name));
    }

    public IReadOnlyList<EndpointMetrics> GetAll()
    {
        return _metrics.Values.OrderBy(metrics => metrics.Name, StringComparer.Ordinal).ToList();
    }
}
