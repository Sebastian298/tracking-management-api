namespace tracking_management_api.Infrastructure.Metrics;

public interface IEndpointMetricsRegistry
{
    EndpointMetrics Get(string endpointName);

    IReadOnlyList<EndpointMetrics> GetAll();
}
