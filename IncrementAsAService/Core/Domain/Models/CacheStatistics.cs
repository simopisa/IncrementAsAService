
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
namespace IncrementAsAService.Core.Domain.Models;

public sealed class CacheStatistics
{
    public long Hits { get; set; }
    public long Misses { get; set; }
    public long Evictions { get; set; }
    public double HitRate => Hits + Misses == 0 ? 0 : (double)Hits / (Hits + Misses);
    public int CurrentSize { get; set; }
}
