using System.Collections.Concurrent;

namespace tracking_management_api.Infrastructure.Metrics;

public sealed class EndpointMetrics
{
    private const int MaxStoredLatencies = 10_000;

    private readonly ConcurrentQueue<double> _latencies = new();
    private long _total;
    private long _success;
    private long _failure;
    private long _timeout;
    private long _completed;
    private long _latencySumMs;
    private int _inFlight;
    private int _peakInFlight;
    private long _minLatencyMs = long.MaxValue;
    private long _maxLatencyMs;

    public EndpointMetrics(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    public string Name { get; }

    public void BeginRequest()
    {
        Interlocked.Increment(ref _total);
        var current = Interlocked.Increment(ref _inFlight);

        int peak;
        while (current > (peak = Volatile.Read(ref _peakInFlight)))
        {
            if (Interlocked.CompareExchange(ref _peakInFlight, current, peak) == peak)
            {
                break;
            }
        }
    }

    public void EndRequest(TimeSpan duration, bool success, bool timeout)
    {
        Interlocked.Decrement(ref _inFlight);
        Interlocked.Increment(ref _completed);

        if (timeout)
        {
            Interlocked.Increment(ref _timeout);
        }
        else if (success)
        {
            Interlocked.Increment(ref _success);
        }
        else
        {
            Interlocked.Increment(ref _failure);
        }

        var latencyMs = Math.Max(0, (long)duration.TotalMilliseconds);
        Interlocked.Add(ref _latencySumMs, latencyMs);
        UpdateMin(latencyMs);
        UpdateMax(latencyMs);

        _latencies.Enqueue(latencyMs);
        while (_latencies.Count > MaxStoredLatencies && _latencies.TryDequeue(out _))
        {
        }
    }

    public EndpointMetricsSnapshot Snapshot()
    {
        var completed = Interlocked.Read(ref _completed);
        var latencySum = Interlocked.Read(ref _latencySumMs);
        var minLatency = Interlocked.Read(ref _minLatencyMs);
        var latencies = _latencies.ToArray();
        Array.Sort(latencies);

        return new EndpointMetricsSnapshot(
            Name,
            Interlocked.Read(ref _total),
            Interlocked.Read(ref _success),
            Interlocked.Read(ref _failure),
            Interlocked.Read(ref _timeout),
            Volatile.Read(ref _inFlight),
            Volatile.Read(ref _peakInFlight),
            completed == 0 || minLatency == long.MaxValue ? 0 : minLatency,
            completed == 0 ? 0 : latencySum / (double)completed,
            Interlocked.Read(ref _maxLatencyMs),
            CalculateP95(latencies));
    }

    private void UpdateMin(long latencyMs)
    {
        var current = Interlocked.Read(ref _minLatencyMs);
        while (latencyMs < current)
        {
            var original = Interlocked.CompareExchange(ref _minLatencyMs, latencyMs, current);
            if (original == current)
            {
                break;
            }

            current = original;
        }
    }

    private void UpdateMax(long latencyMs)
    {
        var current = Interlocked.Read(ref _maxLatencyMs);
        while (latencyMs > current)
        {
            var original = Interlocked.CompareExchange(ref _maxLatencyMs, latencyMs, current);
            if (original == current)
            {
                break;
            }

            current = original;
        }
    }

    private static double CalculateP95(double[] sortedLatencies)
    {
        if (sortedLatencies.Length == 0)
        {
            return 0;
        }

        var index = (int)Math.Ceiling(sortedLatencies.Length * 0.95) - 1;
        index = Math.Clamp(index, 0, sortedLatencies.Length - 1);
        return sortedLatencies[index];
    }
}
