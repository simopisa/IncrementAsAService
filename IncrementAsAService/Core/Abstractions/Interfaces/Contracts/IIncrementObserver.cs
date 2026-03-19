
namespace IncrementAsAService.Core.Abstractions.Interfaces.Contracts;

/// <summary>
/// Observes the sacred act of incrementation.
/// For audit trails, compliance, and existential contemplation.
/// </summary>
public interface IIncrementObserver
{
    Task OnBeforeIncrementAsync(IncrementEvent @event);
    Task OnAfterIncrementAsync(IncrementEvent @event);
    Task OnIncrementFailedAsync(IncrementEvent @event, Exception exception);
    Task OnIncrementRolledBackAsync(IncrementEvent @event, string reason);
}
