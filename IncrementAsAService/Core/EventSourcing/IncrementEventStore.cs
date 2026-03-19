
using IncrementAsAService.Core.Domain.Models;

namespace IncrementAsAService.Core.EventSourcing;


public abstract record IncrementDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public Guid CorrelationId { get; init; }
    public int SequenceNumber { get; init; }
}

public sealed record IncrementRequestedEvent<T>(T Value, string? RequestedBy)
    : IncrementDomainEvent where T : struct;

public sealed record IncrementValidationPassedEvent(string ValidatorName)
    : IncrementDomainEvent;

public sealed record IncrementValidationFailedEvent(string ValidatorName, string Reason)
    : IncrementDomainEvent;

public sealed record IncrementStrategySelectedEvent(string StrategyName)
    : IncrementDomainEvent;

public sealed record IncrementExecutedEvent<T>(T OriginalValue, T NewValue, TimeSpan Duration)
    : IncrementDomainEvent where T : struct;

public sealed record IncrementCacheHitEvent<T>(T Value, T CachedResult)
    : IncrementDomainEvent where T : struct;

public sealed record IncrementRolledBackEvent<T>(T Value, string Reason)
    : IncrementDomainEvent where T : struct;

public sealed record IncrementFailedEvent(string Reason, string? ExceptionType)
    : IncrementDomainEvent;

/// <summary>
/// The event store. Because every n+1 operation is a historic moment
/// that must be recorded for posterity.
/// </summary>
public sealed class IncrementEventStore
{
    private readonly ConcurrentDictionary<Guid, List<IncrementDomainEvent>> _streams = new();
    private readonly ReaderWriterLockSlim _lock = new();
    private int _globalSequence;

    public void Append(Guid streamId, IncrementDomainEvent @event)
    {
        _lock.EnterWriteLock();
        try
        {
            var seq = Interlocked.Increment(ref _globalSequence);
            var enriched = @event with
            {
                SequenceNumber = seq,
                CorrelationId = streamId
            };

            _streams.AddOrUpdate(
                streamId,
                _ => new List<IncrementDomainEvent> { enriched },
                (_, list) => { list.Add(enriched); return list; });
        }
        finally { _lock.ExitWriteLock(); }
    }

    public IReadOnlyList<IncrementDomainEvent> GetStream(Guid streamId)
    {
        _lock.EnterReadLock();
        try
        {
            return _streams.TryGetValue(streamId, out var events)
                ? events.AsReadOnly()
                : Array.Empty<IncrementDomainEvent>();
        }
        finally { _lock.ExitReadLock(); }
    }

    public IReadOnlyList<IncrementDomainEvent> GetAllEvents()
    {
        _lock.EnterReadLock();
        try
        {
            return _streams.Values
                .SelectMany(e => e)
                .OrderBy(e => e.SequenceNumber)
                .ToList()
                .AsReadOnly();
        }
        finally { _lock.ExitReadLock(); }
    }

    public int TotalEvents => _streams.Values.Sum(l => l.Count);
    public int TotalStreams => _streams.Count;
}
