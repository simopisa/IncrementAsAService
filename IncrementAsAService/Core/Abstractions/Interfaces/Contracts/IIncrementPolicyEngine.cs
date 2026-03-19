
namespace IncrementAsAService.Core.Abstractions.Interfaces.Contracts;

/// <summary>
/// The policy engine determines HOW the increment should be performed
/// based on complex business rules that definitely exist.
/// </summary>
public interface IIncrementPolicyEngine<T> where T : struct, IComparable<T>
{
    Task<IncrementPolicy> EvaluatePolicyAsync(
        T value,
        IncrementContext context,
        CancellationToken cancellationToken = default);
}

