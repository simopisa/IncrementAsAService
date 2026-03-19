
namespace IncrementAsAService.Core.Abstractions.Interfaces.Contracts;

/// <summary>
/// Represents the sacred ability to add one to something.
/// This is the north star of our entire architecture.
/// </summary>
public interface IIncrementStrategy<T> where T : struct, IComparable<T>
{
    string StrategyName { get; }
    string StrategyDescription { get; }
    Version StrategyVersion { get; }
    int Priority { get; }
    bool CanHandle(T value);
    Task<IncrementResult<T>> IncrementAsync(
        T value,
        IncrementContext context,
        CancellationToken cancellationToken = default);
}

