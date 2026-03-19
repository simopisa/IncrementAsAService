
namespace IncrementAsAService.Core.Abstractions.Interfaces.Contracts;


/// <summary>
/// Because sometimes you need to undo adding 1.
/// We've all been there.
/// </summary>
public interface IReversibleIncrementStrategy<T> : IIncrementStrategy<T>
    where T : struct, IComparable<T>
{
    Task<IncrementResult<T>> DecrementAsync(
        T value,
        IncrementContext context,
        CancellationToken cancellationToken = default);
}
