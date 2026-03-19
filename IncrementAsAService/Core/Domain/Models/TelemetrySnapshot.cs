
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
namespace IncrementAsAService.Core.Domain.Models;


public sealed class TelemetrySnapshot
{
    public long TotalAttempts { get; set; }
    public long TotalSuccesses { get; set; }
    public long TotalFailures { get; set; }
    public long CacheHits { get; set; }
    public long CacheMisses { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public TimeSpan MaxDuration { get; set; }
    public Dictionary<string, long> StrategyUsage { get; set; } = new();
}

