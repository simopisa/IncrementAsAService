
namespace IncrementAsAService.Core.Abstractions.Interfaces.Contracts;

/// <summary>
/// The fundamental contract for anything that can be incremented.
/// If your type doesn't implement this, it doesn't deserve to be incremented.
/// </summary>
public interface IIncrementable<T> where T : struct, IComparable<T>
{
    T Value { get; }
    IIncrementable<T> WithValue(T newValue);
}

