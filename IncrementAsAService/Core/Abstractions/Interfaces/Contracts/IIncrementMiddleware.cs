
namespace IncrementAsAService.Core.Abstractions.Interfaces.Contracts;

/// <summary>
/// Middleware pipeline for the increment operation.
/// Because if ASP.NET has middleware, so should adding 1.
/// </summary>
public interface IIncrementMiddleware<T> where T : struct, IComparable<T>
{
    int Order { get; }
    Task<IncrementResult<T>> ExecuteAsync(
        T value,
        IncrementContext context,
        Func<T, IncrementContext, Task<IncrementResult<T>>> next,
        CancellationToken cancellationToken = default);
}

