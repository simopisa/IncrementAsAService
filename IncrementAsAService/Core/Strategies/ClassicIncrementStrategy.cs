
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
using IncrementAsAService.Core.Domain.Models;
namespace IncrementAsAService.Core.Strategies;


/// <summary>
/// The classic approach: just use the + operator.
/// Boring, but sometimes you need a reliable workhorse.
/// </summary>
public sealed class ClassicIncrementStrategy : IReversibleIncrementStrategy<int>
{
    public string StrategyName => "Classic";
    public string StrategyDescription => "Uses the '+' operator like a normal person would";
    public Version StrategyVersion => new(1, 0, 0);
    public int Priority => 100;

    public bool CanHandle(int value) => value < int.MaxValue;

    public Task<IncrementResult<int>> IncrementAsync(
        int value, IncrementContext context, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        context.AddAuditEntry($"Classic strategy: Computing {value} + 1 using the '+' operator");

        // The actual work. One line.
        var result = value + 1;

        sw.Stop();
        context.AddAuditEntry($"Classic strategy: Computed result = {result} in {sw.Elapsed}");

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

    public Task<IncrementResult<int>> DecrementAsync(
        int value, IncrementContext context, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        context.AddAuditEntry($"Classic strategy: ROLLBACK — Computing {value} - 1");
        var result = value - 1;
        sw.Stop();

        return Task.FromResult(new IncrementResult<int>
        {
            OriginalValue = value,
            ResultValue = result,
            IsSuccess = true,
            StrategyUsed = $"{StrategyName}_Rollback",
            Duration = sw.Elapsed,
            AuditTrail = context.AuditEntries.ToList()
        });
    }
}
