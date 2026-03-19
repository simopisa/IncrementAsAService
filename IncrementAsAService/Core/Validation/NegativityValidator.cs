



using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;

using IncrementAsAService.Core.Domain.Models;
namespace IncrementAsAService.Core.Validation;


/// <summary>
/// Checks if the number is "too negative" to increment.
/// What's "too negative"? That's configurable, obviously.
/// </summary>
public sealed class NegativityValidator : IIncrementValidator<int>
{
    private readonly int _minimumAllowedValue;
    public string ValidatorName => "NegativityGuard";

    public NegativityValidator(int minimumAllowedValue = int.MinValue)
    {
        _minimumAllowedValue = minimumAllowedValue;
    }

    public Task<ValidationResult> ValidateAsync(
        int value, IncrementContext context, CancellationToken ct = default)
    {
        context.AddAuditEntry($"NegativityGuard: Assessing negativity level of {value}");

        if (value < _minimumAllowedValue)
        {
            return Task.FromResult(ValidationResult.Failure(ValidatorName,
                $"Value {value} is too negative (minimum: {_minimumAllowedValue}). " +
                "This number needs therapy, not incrementing."));
        }

        var result = ValidationResult.Success(ValidatorName);
        if (value < 0)
        {
            context.AddAuditEntry($"NegativityGuard: Value {value} is negative but within " +
                "acceptable parameters. Proceeding with caution.");
        }

        return Task.FromResult(result);
    }
}
