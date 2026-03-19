
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
namespace IncrementAsAService.Core.Domain.Models;

/// <summary>
/// An increment request. Because you can't just pass a number around
/// like some kind of barbarian.
/// </summary>
public sealed class IncrementRequest<T> where T : struct, IComparable<T>
{
    public T Value { get; init; }
    public Guid RequestId { get; init; } = Guid.NewGuid();
    public string? RequestedBy { get; init; }
    public DateTimeOffset RequestedAt { get; init; } = DateTimeOffset.UtcNow;
    public IncrementOptions Options { get; init; } = new();
    public string? Justification { get; init; }
    public Priority Priority { get; init; } = Priority.Normal;

    /// <summary>
    /// Creates a new increment request. The Justification field is optional
    /// but your tech lead will ask why you're incrementing this number in
    /// the PR review anyway.
    /// </summary>
    public static IncrementRequest<T> Create(
        T value,
        string? requestedBy = null,
        string? justification = null,
        Priority priority = Priority.Normal) => new()
        {
            Value = value,
            RequestedBy = requestedBy ?? Environment.UserName,
            Justification = justification ?? "Because I need n+1",
            Priority = priority
        };
}
