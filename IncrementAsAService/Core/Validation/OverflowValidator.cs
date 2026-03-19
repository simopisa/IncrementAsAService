



using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;

using IncrementAsAService.Core.Domain.Models;
namespace IncrementAsAService.Core.Validation;

/// <summary>
/// Makes sure the number won't overflow. Responsible engineering.
/// </summary>
public sealed class OverflowValidator : IIncrementValidator<int>
{
    public string ValidatorName => "OverflowGuard";

    public Task<ValidationResult> ValidateAsync(
        int value, IncrementContext context, CancellationToken ct = default)
    {
        context.AddAuditEntry($"OverflowGuard: Checking if {value} < int.MaxValue ({int.MaxValue})");

        return Task.FromResult(value == int.MaxValue
            ? ValidationResult.Failure(ValidatorName,
                $"Cannot increment {value}: would cause integer overflow. " +
                "Consider upgrading to IncrementAsAService.BigInteger.Enterprise™.")
            : ValidationResult.Success(ValidatorName));
    }
}
