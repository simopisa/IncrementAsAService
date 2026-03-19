

using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
namespace IncrementAsAService.Core.Domain.Models;

/// <summary>
/// The result of an increment operation. Not just n+1, but n+1 with
/// provenance, audit trail, timing data, and emotional context.
/// </summary>
public sealed record IncrementResult<T> where T : struct, IComparable<T>
{
    public T OriginalValue { get; init; }
    public T ResultValue { get; init; }
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public string StrategyUsed { get; init; } = "Unknown";
    public TimeSpan Duration { get; init; }
    public Guid OperationId { get; init; } = Guid.NewGuid();
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public int RetryCount { get; init; }
    public bool WasCached { get; init; }
    public IReadOnlyList<string> AuditTrail { get; init; } = Array.Empty<string>();
    public ConfidenceLevel Confidence { get; init; } = ConfidenceLevel.Absolute;
    public IncrementPolicy? AppliedPolicy { get; init; }

    public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });
}
