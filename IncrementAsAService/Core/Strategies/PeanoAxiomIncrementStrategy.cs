
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
using IncrementAsAService.Core.Domain.Models;
namespace IncrementAsAService.Core.Strategies;


/// <summary>
/// Proves n+1 from first principles using the Peano axioms.
/// Takes O(n) time because mathematical rigor has a cost.
/// </summary>
public sealed class PeanoAxiomIncrementStrategy : IIncrementStrategy<int>
{
    public string StrategyName => "PeanoAxiom";
    public string StrategyDescription =>
        "Derives the successor function from Peano axioms. O(n) time but O(∞) intellectual rigor.";
    public Version StrategyVersion => new(1, 0, 0);
    public int Priority => 10;

    public bool CanHandle(int value) => value >= 0 && value < 10000; // Peano is slow, okay?

    public Task<IncrementResult<int>> IncrementAsync(
        int value, IncrementContext context, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        context.AddAuditEntry($"Peano strategy: Constructing natural number {value} from zero");
        context.AddAuditEntry($"Peano strategy: Axiom 1 — 0 is a natural number ✓");
        context.AddAuditEntry($"Peano strategy: Axiom 2 — For every natural number n, S(n) is a natural number ✓");

        // Build n from zero by applying successor function n times
        int current = 0;
        for (int i = 0; i < value; i++)
        {
            if (ct.IsCancellationRequested)
            {
                return Task.FromResult(new IncrementResult<int>
                {
                    OriginalValue = value,
                    ResultValue = value,
                    IsSuccess = false,
                    ErrorMessage = "Peano proof cancelled. Mathematical truth waits for no one.",
                    StrategyUsed = StrategyName,
                    Duration = sw.Elapsed
                });
            }
            current++; // S(current)
        }

        context.AddAuditEntry($"Peano strategy: Successfully constructed {value} via {value} applications of S()");
        context.AddAuditEntry($"Peano strategy: Applying S() one more time...");

        // Apply successor one more time — the actual increment
        current++;

        sw.Stop();
        context.AddAuditEntry($"Peano strategy: S({value}) = {current}. QED.");
        context.AddAuditEntry($"Peano strategy: Proof completed in {sw.Elapsed}. " +
            $"Total successor applications: {value + 1}");

        return Task.FromResult(new IncrementResult<int>
        {
            OriginalValue = value,
            ResultValue = current,
            IsSuccess = true,
            StrategyUsed = StrategyName,
            Duration = sw.Elapsed,
            Confidence = ConfidenceLevel.Absolute,
            AuditTrail = context.AuditEntries.ToList()
        });
    }
}

