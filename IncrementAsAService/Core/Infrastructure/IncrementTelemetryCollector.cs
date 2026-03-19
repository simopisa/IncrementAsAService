
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
using IncrementAsAService.Core.Domain.Models;

namespace IncrementAsAService.Core.Infrastructure;

/// <summary>
/// Telemetry collector for monitoring increment operations.
/// Hook this up to Grafana and watch your n+1 throughput in real-time.
/// </summary>
public sealed class IncrementTelemetryCollector : IIncrementTelemetry
{
    private long _attempts;
    private long _successes;
    private long _failures;
    private long _cacheHits;
    private long _cacheMisses;
    private readonly ConcurrentDictionary<string, long> _strategyUsage = new();
    private readonly ConcurrentBag<TimeSpan> _durations = new();

    public void RecordIncrementAttempt(string strategyName)
    {
        Interlocked.Increment(ref _attempts);
        _strategyUsage.AddOrUpdate(strategyName, 1, (_, v) => v + 1);
    }

    public void RecordIncrementSuccess(string strategyName, TimeSpan duration)
    {
        Interlocked.Increment(ref _successes);
        _durations.Add(duration);
    }

    public void RecordIncrementFailure(string strategyName, Exception exception)
    {
        Interlocked.Increment(ref _failures);
    }

    public void RecordCacheHit() => Interlocked.Increment(ref _cacheHits);
    public void RecordCacheMiss() => Interlocked.Increment(ref _cacheMisses);

    public TelemetrySnapshot GetSnapshot() => new()
    {
        TotalAttempts = _attempts,
        TotalSuccesses = _successes,
        TotalFailures = _failures,
        CacheHits = _cacheHits,
        CacheMisses = _cacheMisses,
        AverageDuration = _durations.Any()
            ? TimeSpan.FromTicks((long)_durations.Average(d => d.Ticks))
            : TimeSpan.Zero,
        MaxDuration = _durations.Any()
            ? _durations.Max()
            : TimeSpan.Zero,
        StrategyUsage = new Dictionary<string, long>(_strategyUsage)
    };
}

