
namespace IncrementAsAService.Core.Abstractions.Interfaces.Contracts;

/// <summary>
/// Caches increment results because calculating n+1 is computationally
/// expensive and we can't afford to do it twice.
/// </summary>
public interface IIncrementCache<T> where T : struct, IComparable<T>
{
    Task<IncrementResult<T>?> GetCachedResultAsync(T value);
    Task SetCachedResultAsync(T value, IncrementResult<T> result, TimeSpan? ttl = null);
    Task InvalidateAsync(T value);
    Task FlushAsync();
    CacheStatistics GetStatistics();
}
