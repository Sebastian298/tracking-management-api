using System.Diagnostics;
using Microsoft.Extensions.Options;
using tracking_management_api.Services.Loads;

namespace tracking_management_api.Infrastructure.Metrics;

public sealed class MetricsReporterService : BackgroundService
{
    private readonly IEndpointMetricsRegistry _registry;
    private readonly ILoadCacheStats _cacheStats;
    private readonly ILogger<MetricsReporterService> _logger;
    private readonly EndpointMetricsOptions _options;
    private readonly Dictionary<string, long> _previousTotals = new(StringComparer.Ordinal);

    public MetricsReporterService(
        IEndpointMetricsRegistry registry,
        ILoadCacheStats cacheStats,
        IOptions<EndpointMetricsOptions> options,
        ILogger<MetricsReporterService> logger)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(cacheStats);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _registry = registry;
        _cacheStats = cacheStats;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(1, _options.ReportIntervalSeconds));
        using var timer = new PeriodicTimer(interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            Report(interval.TotalSeconds);
        }
    }

    private void Report(double intervalSeconds)
    {
        var process = Process.GetCurrentProcess();
        var heapBytes = GC.GetTotalMemory(forceFullCollection: false);
        ThreadPool.GetAvailableThreads(out var workerAvailable, out _);
        ThreadPool.GetMaxThreads(out var workerMax, out _);
        var workerBusy = Math.Max(0, workerMax - workerAvailable);

        var endpoints = _registry.GetAll();
        if (endpoints.Count == 0)
        {
            _logger.LogInformation(
                "Endpoint stats: waiting for traffic | heapMB={HeapMb:F1} workingSetMB={WorkingSetMb:F1} tpBusy={WorkerBusy}/{WorkerMax}",
                BytesToMegabytes(heapBytes),
                BytesToMegabytes(process.WorkingSet64),
                workerBusy,
                workerMax);
            return;
        }

        foreach (var metrics in endpoints)
        {
            var snapshot = metrics.Snapshot();
            var previousTotal = _previousTotals.GetValueOrDefault(snapshot.Name);
            var intervalRequests = Math.Max(0, snapshot.Total - previousTotal);
            var requestsPerSecond = intervalSeconds > 0 ? intervalRequests / intervalSeconds : 0;
            _previousTotals[snapshot.Name] = snapshot.Total;

            var isCachedEndpoint = string.Equals(snapshot.Name, "loads.cached", StringComparison.Ordinal);

            _logger.LogInformation(
                "Endpoint stats: name={Name} total={Total} success={Success} fail={Failure} timeout={Timeout} inFlight={InFlight} peakInFlight={PeakInFlight} rps={Rps:F2} latencyMs min={Min:F0} avg={Avg:F0} max={Max:F0} p95={P95:F0} heapMB={HeapMb:F1} workingSetMB={WorkingSetMb:F1} tpBusy={WorkerBusy}/{WorkerMax} cacheHits={CacheHits} cacheMisses={CacheMisses}",
                snapshot.Name,
                snapshot.Total,
                snapshot.Success,
                snapshot.Failure,
                snapshot.Timeout,
                snapshot.InFlight,
                snapshot.PeakInFlight,
                requestsPerSecond,
                snapshot.MinLatencyMs,
                snapshot.AverageLatencyMs,
                snapshot.MaxLatencyMs,
                snapshot.P95LatencyMs,
                BytesToMegabytes(heapBytes),
                BytesToMegabytes(process.WorkingSet64),
                workerBusy,
                workerMax,
                isCachedEndpoint ? _cacheStats.Hits.ToString() : "n/a",
                isCachedEndpoint ? _cacheStats.Misses.ToString() : "n/a");
        }
    }

    private static double BytesToMegabytes(long bytes)
    {
        return bytes / 1024d / 1024d;
    }
}
