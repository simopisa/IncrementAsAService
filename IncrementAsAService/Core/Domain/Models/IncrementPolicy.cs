
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
namespace IncrementAsAService.Core.Domain.Models;


public sealed class IncrementPolicy
{
    public string PolicyName { get; init; } = "DefaultIncrementPolicy";
    public bool RequiresApproval { get; init; }
    public int MaxDailyIncrements { get; init; } = int.MaxValue;
    public bool AllowNegativeNumbers { get; init; } = true;
    public bool AllowZero { get; init; } = true;
    public TimeSpan CooldownPeriod { get; init; } = TimeSpan.Zero;
    public string? ApprovedBy { get; init; }
}
