
namespace IncrementAsAService.Core.Abstractions.Interfaces.Contracts;

/// <summary>
/// Resolves conflicts when multiple increment strategies disagree
/// on how to add 1 to a number. Yes, this is a real concern apparently.
/// </summary>
public interface IIncrementConflictResolver<T> where T : struct, IComparable<T>
{
    Task<IncrementResult<T>> ResolveAsync(
        IReadOnlyList<IncrementResult<T>> conflictingResults,
        IncrementContext context,
        CancellationToken cancellationToken = default);
}

