
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
namespace IncrementAsAService.Core.Domain.Models;


/// <summary>
/// Wraps a number with the full ceremony it deserves.
/// Raw ints are for amateurs.
/// </summary>
public sealed class IncrementableValue<T> : IIncrementable<T>, IEquatable<IncrementableValue<T>>
    where T : struct, IComparable<T>
{
    public T Value { get; }
    public Guid InstanceId { get; }
    public DateTimeOffset CreatedAt { get; }
    public string? Label { get; }
    public IReadOnlyDictionary<string, object> Metadata { get; }

    public IncrementableValue(
        T value,
        string? label = null,
        IDictionary<string, object>? metadata = null)
    {
        Value = value;
        InstanceId = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
        Label = label ?? $"Value_{InstanceId:N}";
        Metadata = new Dictionary<string, object>(metadata ?? new Dictionary<string, object>());
    }

    public IIncrementable<T> WithValue(T newValue) =>
        new IncrementableValue<T>(newValue, Label, Metadata as IDictionary<string, object>);

    public bool Equals(IncrementableValue<T>? other) =>
        other is not null && EqualityComparer<T>.Default.Equals(Value, other.Value);

    public override bool Equals(object? obj) => Equals(obj as IncrementableValue<T>);
    public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(Value);
    public override string ToString() => $"IncrementableValue<{typeof(T).Name}>({Value}) [ID: {InstanceId}]";
}

