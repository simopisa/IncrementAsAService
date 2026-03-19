
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
namespace IncrementAsAService.Core.Domain.Models;

public sealed class IncrementOptions
{
    public bool EnableCaching { get; init; } = true;
    public bool EnableAuditTrail { get; init; } = true;
    public bool EnableTelemetry { get; init; } = true;
    public bool AllowRollback { get; init; } = true;
    public int MaxRetries { get; init; } = 3;
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    public RetryPolicy RetryPolicy { get; init; } = RetryPolicy.ExponentialBackoff;
    public string? PreferredStrategy { get; init; }
    public bool RunValidation { get; init; } = true;
    public bool RequireConsensus { get; init; } = false;
}


