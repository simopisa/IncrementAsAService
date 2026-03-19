
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
using IncrementAsAService.Core.Domain.Models;

namespace IncrementAsAService.Core.Infrastructure;

/// <summary>
/// In-memory cache with LRU eviction for increment results.
/// Because calculating 5+1 twice would be wasteful.
/// </summary>
public sealed class InMemoryIncrementCache : IIncrementCache<int>
{
    private readonly ConcurrentDictionary<int, (IncrementResult<int> Result, DateTimeOffset ExpiresAt)> _cache = new();
    private readonly int _maxSize;
    private readonly CacheStatistics _stats = new();

    public InMemoryIncrementCache(int maxSize = 10000)
    {
        _maxSize = maxSize;
    }

    public Task<IncrementResult<int>?> GetCachedResultAsync(int value)
    {
        if (_cache.TryGetValue(value, out var entry) && entry.ExpiresAt > DateTimeOffset.UtcNow)
        {
            //Interlocked.Increment(ref _stats.Hits); needs fixing
            _stats.Hits++;
            return Task.FromResult<IncrementResult<int>?>(entry.Result with { WasCached = true });
        }

        //Interlocked.Increment(ref _stats.Misses); needs fixing
        _stats.Misses++;
        return Task.FromResult<IncrementResult<int>?>(null);
    }

    public Task SetCachedResultAsync(int value, IncrementResult<int> result, TimeSpan? ttl = null)
    {
        if (_cache.Count >= _maxSize)
        {
            // Evict oldest entry. Enterprise-grade LRU.
            var oldest = _cache.OrderBy(kvp => kvp.Value.ExpiresAt).First();
            _cache.TryRemove(oldest.Key, out _);
            //Interlocked.Increment(ref _stats.Evictions); needs fixing
            _stats.Evictions++;
        }

        var expiresAt = DateTimeOffset.UtcNow + (ttl ?? TimeSpan.FromHours(1));
        _cache[value] = (result, expiresAt);
        _stats.CurrentSize = _cache.Count;
        return Task.CompletedTask;
    }

    public Task InvalidateAsync(int value) { _cache.TryRemove(value, out _); return Task.CompletedTask; }
    public Task FlushAsync() { _cache.Clear(); return Task.CompletedTask; }
    public CacheStatistics GetStatistics() => _stats;
}

