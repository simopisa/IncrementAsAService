
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
namespace IncrementAsAService.Core.Domain.Models;


public sealed class BatchIncrementResult<T> where T : struct, IComparable<T>
{
    public IReadOnlyList<IncrementResult<T>> Results { get; init; } = Array.Empty<IncrementResult<T>>();
    public int TotalRequests => Results.Count;
    public int SuccessCount => Results.Count(r => r.IsSuccess);
    public int FailureCount => Results.Count(r => !r.IsSuccess);
    public TimeSpan TotalDuration { get; init; }
    public double SuccessRate => TotalRequests == 0 ? 0 : (double)SuccessCount / TotalRequests;
}

