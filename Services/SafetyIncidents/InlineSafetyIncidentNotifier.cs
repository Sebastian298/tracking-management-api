using Microsoft.Extensions.Options;
using tracking_management_api.Dtos;
using tracking_management_api.Options.SafetyIncidents;

namespace tracking_management_api.Services.SafetyIncidents;

public sealed class InlineSafetyIncidentNotifier : ISafetyIncidentNotifier
{
    private readonly SafetyIncidentsOptions _options;
    private readonly ILogger<InlineSafetyIncidentNotifier> _logger;

    public InlineSafetyIncidentNotifier(
        IOptions<SafetyIncidentsOptions> options,
        ILogger<InlineSafetyIncidentNotifier> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _logger = logger;
    }

    public async Task<Guid> NotifyAsync(SafetyIncidentDto incident, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(incident);

        var delay = Math.Max(0, _options.ProcessingDelayMs);
        await Task.Delay(delay, cancellationToken);

        _logger.LogInformation("Safety incident {IncidentId} processed inline.", incident.IncidentId);
        return Guid.Empty;
    }
}
