using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tracking_management_api.Dtos;
using tracking_management_api.Infrastructure.Messaging;
using tracking_management_api.Infrastructure.Metrics;
using tracking_management_api.Services.SafetyIncidents;

namespace tracking_management_api.Controllers;

[ApiController]
[Route("api/safety-incidents")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class SafetyIncidentsController : ControllerBase
{
    private readonly ISafetyIncidentService _service;
    private readonly ISafetyIncidentNotifier _inlineNotifier;
    private readonly ISafetyIncidentNotifier _rabbitMqNotifier;
    private readonly ILogger<SafetyIncidentsController> _logger;

    public SafetyIncidentsController(
        ISafetyIncidentService service,
        [FromKeyedServices(SafetyIncidentNotifierKeys.Inline)] ISafetyIncidentNotifier inlineNotifier,
        [FromKeyedServices(SafetyIncidentNotifierKeys.RabbitMq)] ISafetyIncidentNotifier rabbitMqNotifier,
        ILogger<SafetyIncidentsController> logger)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(inlineNotifier);
        ArgumentNullException.ThrowIfNull(rabbitMqNotifier);
        ArgumentNullException.ThrowIfNull(logger);

        _service = service;
        _inlineNotifier = inlineNotifier;
        _rabbitMqNotifier = rabbitMqNotifier;
        _logger = logger;
    }

    [HttpPost("direct")]
    [TrackEndpointMetrics("safety-incidents.direct")]
    [ProducesResponseType(typeof(SafetyIncidentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SafetyIncidentDto>> CreateDirectAsync(
        [FromBody] CreateSafetyIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var incident = await _service.RegisterAsync(request, cancellationToken);
        await _inlineNotifier.NotifyAsync(incident, cancellationToken);
        stopwatch.Stop();

        WriteProcessingHeaders("inline", stopwatch.ElapsedMilliseconds);
        return StatusCode(StatusCodes.Status201Created, incident);
    }

    [HttpPost("queued")]
    [TrackEndpointMetrics("safety-incidents.queued")]
    [ProducesResponseType(typeof(SafetyIncidentDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<SafetyIncidentDto>> CreateQueuedAsync(
        [FromBody] CreateSafetyIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var incident = await _service.RegisterAsync(request, cancellationToken);

        Guid messageId;
        try
        {
            messageId = await _rabbitMqNotifier.NotifyAsync(incident, cancellationToken);
        }
        catch (SafetyIncidentPublishException exception)
        {
            stopwatch.Stop();
            WriteProcessingHeaders("queued", stopwatch.ElapsedMilliseconds);
            _logger.LogError(
                exception,
                "Failed to queue notification for safety incident {IncidentId}.",
                incident.IncidentId);

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new ProblemDetails
                {
                    Status = StatusCodes.Status503ServiceUnavailable,
                    Title = "Message broker unavailable",
                    Detail = "The safety incident was saved but the notification could not be queued."
                });
        }

        stopwatch.Stop();
        WriteProcessingHeaders("queued", stopwatch.ElapsedMilliseconds);
        Response.Headers["X-Message-Id"] = messageId.ToString();
        return Accepted(incident);
    }

    private void WriteProcessingHeaders(string processing, long elapsedMs)
    {
        Response.Headers["X-Processing"] = processing;
        Response.Headers["X-Elapsed-Ms"] = elapsedMs.ToString();
    }
}
