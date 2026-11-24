using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tracking_management_api.Dtos;
using tracking_management_api.Infrastructure.Metrics;
using tracking_management_api.Services.Loads;

namespace tracking_management_api.Controllers;

[ApiController]
[Route("api/loads")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class LoadsController : ControllerBase
{
    private readonly ILoadService _loadService;
    private readonly ICachedLoadService _cachedLoadService;

    public LoadsController(ILoadService loadService, ICachedLoadService cachedLoadService)
    {
        ArgumentNullException.ThrowIfNull(loadService);
        ArgumentNullException.ThrowIfNull(cachedLoadService);

        _loadService = loadService;
        _cachedLoadService = cachedLoadService;
    }

    [HttpGet("direct")]
    [TrackEndpointMetrics("loads.direct")]
    [ProducesResponseType(typeof(IReadOnlyList<LoadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LoadDto>>> GetDirectAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var loads = await _loadService.GetAllAsync(cancellationToken);
        stopwatch.Stop();
        WriteSourceHeaders("csv", fromCache: false, stopwatch.ElapsedMilliseconds);
        return Ok(loads);
    }

    [HttpGet("cached")]
    [TrackEndpointMetrics("loads.cached")]
    [ProducesResponseType(typeof(IReadOnlyList<LoadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LoadDto>>> GetCachedAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await _cachedLoadService.GetAllAsync(cancellationToken);
        stopwatch.Stop();
        WriteSourceHeaders(
            result.FromCache ? "memory-cache" : "csv",
            result.FromCache,
            stopwatch.ElapsedMilliseconds);
        return Ok(result.Loads);
    }

    private void WriteSourceHeaders(string source, bool fromCache, long elapsedMs)
    {
        Response.Headers["X-Data-Source"] = source;
        Response.Headers["X-From-Cache"] = fromCache ? "true" : "false";
        Response.Headers["X-Source-Elapsed-Ms"] = elapsedMs.ToString();
    }
}
