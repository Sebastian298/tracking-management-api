using tracking_management_api.Infrastructure.Metrics;

namespace tracking_management_api.Tests;

public sealed class EndpointMetricsTests
{
    [Fact]
    public async Task ConcurrentRequests_TrackTotalsAndPeakInFlight()
    {
        var metrics = new EndpointMetrics("loads.direct");
        var tasks = Enumerable.Range(0, 100).Select(async _ =>
        {
            metrics.BeginRequest();
            await Task.Delay(1);
            metrics.EndRequest(TimeSpan.FromMilliseconds(10), success: true, timeout: false);
        });

        await Task.WhenAll(tasks);

        var snapshot = metrics.Snapshot();
        Assert.Equal(100, snapshot.Total);
        Assert.Equal(100, snapshot.Success);
        Assert.Equal(0, snapshot.Failure);
        Assert.Equal(0, snapshot.Timeout);
        Assert.Equal(0, snapshot.InFlight);
        Assert.True(snapshot.PeakInFlight >= 1);
        Assert.True(snapshot.PeakInFlight <= 100);
    }

    [Fact]
    public void Snapshot_ComputesP95AndMinMaxLatency()
    {
        var metrics = new EndpointMetrics("loads.cached");
        for (var latencyMs = 1; latencyMs <= 20; latencyMs++)
        {
            metrics.BeginRequest();
            metrics.EndRequest(TimeSpan.FromMilliseconds(latencyMs), success: true, timeout: false);
        }

        var snapshot = metrics.Snapshot();

        Assert.Equal(1, snapshot.MinLatencyMs);
        Assert.Equal(20, snapshot.MaxLatencyMs);
        Assert.Equal(19, snapshot.P95LatencyMs);
        Assert.Equal(10.5, snapshot.AverageLatencyMs);
    }

    [Fact]
    public void EndRequest_ClassifiesTimeoutsAndFailures()
    {
        var metrics = new EndpointMetrics("loads.cached");

        metrics.BeginRequest();
        metrics.EndRequest(TimeSpan.FromMilliseconds(30_000), success: false, timeout: true);

        metrics.BeginRequest();
        metrics.EndRequest(TimeSpan.FromMilliseconds(50), success: false, timeout: false);

        metrics.BeginRequest();
        metrics.EndRequest(TimeSpan.FromMilliseconds(80), success: true, timeout: false);

        var snapshot = metrics.Snapshot();
        Assert.Equal(3, snapshot.Total);
        Assert.Equal(1, snapshot.Success);
        Assert.Equal(1, snapshot.Failure);
        Assert.Equal(1, snapshot.Timeout);
    }

    [Fact]
    public void Snapshot_WhenThereAreNoSamples_ReturnsZeroLatencies()
    {
        var snapshot = new EndpointMetrics("loads.direct").Snapshot();

        Assert.Equal(0, snapshot.MinLatencyMs);
        Assert.Equal(0, snapshot.AverageLatencyMs);
        Assert.Equal(0, snapshot.MaxLatencyMs);
        Assert.Equal(0, snapshot.P95LatencyMs);
    }
}
