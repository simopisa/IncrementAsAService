
using IncrementAsAService.Core.Domain.Models;
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
namespace IncrementAsAService.Core.Strategies;


/// <summary>
/// Increments by subtracting negative one.
/// Mathematically equivalent. Spiritually different.
/// </summary>
public sealed class DoubleNegationIncrementStrategy : IIncrementStrategy<int>
{
    public string StrategyName => "DoubleNegation";
    public string StrategyDescription =>
        "Computes n - (-1) because subtraction of negatives is addition. Mind = blown.";
    public Version StrategyVersion => new(1, 0, 0);
    public int Priority => 30;

    public bool CanHandle(int value) => value < int.MaxValue;

    public Task<IncrementResult<int>> IncrementAsync(
        int value, IncrementContext context, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        context.AddAuditEntry($"DoubleNegation strategy: Preparing to subtract negative one from {value}");
        context.AddAuditEntry($"DoubleNegation strategy: Computing -(-1) = {-(-1)}");
        context.AddAuditEntry($"DoubleNegation strategy: Therefore {value} - (-1) = {value} + 1");

        var result = value - (-1);

        sw.Stop();
        context.AddAuditEntry($"DoubleNegation strategy: QED. Result = {result}");

        return Task.FromResult(new IncrementResult<int>
        {
            OriginalValue = value,
            ResultValue = result,
            IsSuccess = true,
            StrategyUsed = StrategyName,
            Duration = sw.Elapsed,
            AuditTrail = context.AuditEntries.ToList()
        });
    }
}

