using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace tracking_management_api.Infrastructure.Metrics;

[AttributeUsage(AttributeTargets.Method)]
public sealed class TrackEndpointMetricsAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _endpointName;

    public TrackEndpointMetricsAttribute(string endpointName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpointName);
        _endpointName = endpointName;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var registry = context.HttpContext.RequestServices.GetRequiredService<IEndpointMetricsRegistry>();
        var metrics = registry.Get(_endpointName);
        metrics.BeginRequest();

        var stopwatch = Stopwatch.StartNew();
        ActionExecutedContext? executed = null;
        try
        {
            executed = await next();
        }
        finally
        {
            stopwatch.Stop();
            var (success, timeout) = Classify(executed);
            metrics.EndRequest(stopwatch.Elapsed, success, timeout);
        }
    }

    private static (bool Success, bool Timeout) Classify(ActionExecutedContext? executed)
    {
        if (executed is null)
        {
            return (false, false);
        }

        if (executed.Exception is not null && !executed.ExceptionHandled)
        {
            return (false, false);
        }

        var statusCode = ResolveStatusCode(executed);
        return (statusCode is >= 200 and < 300, statusCode == StatusCodes.Status504GatewayTimeout);
    }

    private static int ResolveStatusCode(ActionExecutedContext executed)
    {
        return executed.Result switch
        {
            ObjectResult objectResult => objectResult.StatusCode ?? StatusCodes.Status200OK,
            StatusCodeResult statusCodeResult => statusCodeResult.StatusCode,
            EmptyResult => StatusCodes.Status200OK,
            _ => executed.HttpContext.Response.StatusCode
        };
    }
}
