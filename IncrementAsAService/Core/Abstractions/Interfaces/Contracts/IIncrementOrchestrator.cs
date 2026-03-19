
namespace IncrementAsAService.Core.Abstractions.Interfaces.Contracts;

/// <summary>
/// The top-level orchestrator. This is the GOD class that coordinates
/// the entire increment operation across all subsystems.
/// </summary>
public interface IIncrementOrchestrator<T> where T : struct, IComparable<T>
{
    Task<IncrementResult<T>> OrchestrateAsync(
        IncrementRequest<T> request,
        CancellationToken cancellationToken = default);
    Task<BatchIncrementResult<T>> OrchestrateBatchAsync(
        IEnumerable<IncrementRequest<T>> requests,
        BatchOptions options,
        CancellationToken cancellationToken = default);
    Task<IncrementResult<T>> OrchestrateWithRollbackAsync(
        IncrementRequest<T> request,
        CancellationToken cancellationToken = default);
}

