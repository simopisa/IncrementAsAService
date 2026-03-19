
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
namespace IncrementAsAService.Core.Domain.Models;

public sealed class BatchOptions
{
    public int MaxDegreeOfParallelism { get; init; } = Environment.ProcessorCount;
    public bool StopOnFirstFailure { get; init; }
    public TimeSpan BatchTimeout { get; init; } = TimeSpan.FromMinutes(5);
}
