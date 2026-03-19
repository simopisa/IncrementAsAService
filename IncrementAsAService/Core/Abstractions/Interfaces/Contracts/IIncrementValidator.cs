
namespace IncrementAsAService.Core.Abstractions.Interfaces.Contracts;

/// <summary>
/// Validates whether a number is WORTHY of being incremented.
/// Not every number deserves this privilege.
/// </summary>
public interface IIncrementValidator<T> where T : struct, IComparable<T>
{
    string ValidatorName { get; }
    Task<ValidationResult> ValidateAsync(
        T value,
        IncrementContext context,
        CancellationToken cancellationToken = default);
}
