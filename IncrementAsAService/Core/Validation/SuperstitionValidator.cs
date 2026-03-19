



using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;

using IncrementAsAService.Core.Domain.Models;
namespace IncrementAsAService.Core.Validation;

/// <summary>
/// Validates that the number isn't cursed (13, 666, 4 in East Asian numerology).
/// Safety first.
/// </summary>
public sealed class SuperstitionValidator : IIncrementValidator<int>
{
    private static readonly HashSet<int> _cursedNumbers = new() { 13, 666, 4, 9, 17, 39, 87 };
    public string ValidatorName => "SuperstitionGuard";

    public Task<ValidationResult> ValidateAsync(
        int value, IncrementContext context, CancellationToken ct = default)
    {
        context.AddAuditEntry($"SuperstitionGuard: Consulting numerological databases...");

        if (_cursedNumbers.Contains(value))
        {
            context.AddAuditEntry($"SuperstitionGuard: [WARNING] — {value} is a cursed number!");
            return Task.FromResult(new ValidationResult
            {
                IsValid = true, // I allow it, but with warnings
                ValidatorName = ValidatorName,
                Warnings = new[] { $"Value {value} is considered unlucky in some cultures. " +
                        "Increment at your own risk. We are not liable for any bad luck." }
            });
        }

        context.AddAuditEntry($"SuperstitionGuard: {value} appears to be numerologically safe. ✓");
        return Task.FromResult(ValidationResult.Success(ValidatorName));
    }
}

