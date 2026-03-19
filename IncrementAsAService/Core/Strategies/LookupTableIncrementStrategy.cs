
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
using IncrementAsAService.Core.Domain.Models;
namespace IncrementAsAService.Core.Strategies;



/// <summary>
/// Uses a lookup table for "fast" increments.
/// Pre-computes the answers because why do math at runtime?
/// </summary>
public sealed class LookupTableIncrementStrategy : IIncrementStrategy<int>
{
    public string StrategyName => "LookupTable";
    public string StrategyDescription =>
        "Pre-computed lookup table. O(1) time, O(my god that's a lot of memory) space.";
    public Version StrategyVersion => new(3, 0, 0);
    public int Priority => 80;

    // Pre-compute a modest lookup table
    private static readonly Dictionary<int, int> _lookupTable;

    static LookupTableIncrementStrategy()
    {
        _lookupTable = new Dictionary<int, int>();
        for (int i = -1000; i <= 1000; i++)
            _lookupTable[i] = i + 1; // I know I used + here, but it's only at startup.
    }

    public bool CanHandle(int value) => _lookupTable.ContainsKey(value);

    public Task<IncrementResult<int>> IncrementAsync(
        int value, IncrementContext context, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        context.AddAuditEntry($"LookupTable strategy: Searching pre-computed table " +
            $"({_lookupTable.Count} entries) for value {value}");

        if (!_lookupTable.TryGetValue(value, out var result))
        {
            sw.Stop();
            return Task.FromResult(new IncrementResult<int>
            {
                OriginalValue = value,
                ResultValue = value,
                IsSuccess = false,
                ErrorMessage = $"Value {value} not found in lookup table. " +
                    "Please submit a JIRA ticket to expand the table.",
                StrategyUsed = StrategyName,
                Duration = sw.Elapsed
            });
        }

        sw.Stop();
        context.AddAuditEntry($"LookupTable strategy: Cache hit! {value} → {result}");

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



