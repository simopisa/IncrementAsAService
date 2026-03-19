
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
namespace IncrementAsAService.Core.Domain.Models;

public sealed class IncrementContext
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public Guid CausationId { get; init; }
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;
    public string? RequestedBy { get; init; }
    public IncrementOptions Options { get; init; } = new();
    public IncrementPolicy? ActivePolicy { get; init; }
    public ConcurrentDictionary<string, object> Items { get; } = new();
    public List<string> AuditEntries { get; } = new();

    public void AddAuditEntry(string entry) =>
        AuditEntries.Add($"[{DateTimeOffset.UtcNow:O}] [{CorrelationId:N}] {entry}");
}



