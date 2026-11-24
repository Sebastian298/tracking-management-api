namespace tracking_management_api.Infrastructure.Metrics;

public sealed class EndpointMetricsOptions
{
    public const string SectionName = "EndpointMetrics";

    public int ReportIntervalSeconds { get; set; } = 10;
}
