
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
namespace IncrementAsAService.Core.Domain.Models;


public sealed class IncrementEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType { get; init; } = "";
    public object? OriginalValue { get; init; }
    public object? ResultValue { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public string? StrategyUsed { get; init; }
    public IReadOnlyDictionary<string, object> Properties { get; init; } =
        new Dictionary<string, object>();
}
