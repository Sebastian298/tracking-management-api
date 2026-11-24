namespace tracking_management_api.Infrastructure.Metrics;

public sealed record EndpointMetricsSnapshot(
    string Name,
    long Total,
    long Success,
    long Failure,
    long Timeout,
    int InFlight,
    int PeakInFlight,
    double MinLatencyMs,
    double AverageLatencyMs,
    double MaxLatencyMs,
    double P95LatencyMs);
