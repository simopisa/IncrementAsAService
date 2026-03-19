
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
namespace IncrementAsAService.Core.Domain.Models;


public sealed class ValidationResult
{
    public bool IsValid { get; init; }
    public string? ValidatorName { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();

    public static ValidationResult Success(string validator) =>
        new() { IsValid = true, ValidatorName = validator };
    public static ValidationResult Failure(string validator, params string[] errors) =>
        new() { IsValid = false, ValidatorName = validator, Errors = errors };
}


