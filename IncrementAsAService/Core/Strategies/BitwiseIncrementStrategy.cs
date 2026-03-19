
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
using IncrementAsAService.Core.Domain.Models;
namespace IncrementAsAService.Core.Strategies;



/// <summary>
/// Uses bitwise operations to increment. Because why use '+' when you
/// can confuse everyone in the code review?
/// </summary>
public sealed class BitwiseIncrementStrategy : IIncrementStrategy<int>
{
    public string StrategyName => "Bitwise";
    public string StrategyDescription =>
        "Uses bitwise XOR and AND operations because we're showing off";
    public Version StrategyVersion => new(2, 1, 0);
    public int Priority => 50;

    public bool CanHandle(int value) => value < int.MaxValue;

    public Task<IncrementResult<int>> IncrementAsync(
        int value, IncrementContext context, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        context.AddAuditEntry($"Bitwise strategy: Initiating bit manipulation on {value}");
        context.AddAuditEntry($"Bitwise strategy: Binary representation = {Convert.ToString(value, 2)}");

        int carry = 1;
        int result = value;
        int iterations = 0;

        while (carry != 0)
        {
            iterations++;
            int temp = result ^ carry;
            carry = (result & carry) << 1;
            result = temp;
            context.AddAuditEntry($"Bitwise strategy: Iteration {iterations} — " +
                $"intermediate = {result}, carry = {carry}");
        }

        sw.Stop();
        context.AddAuditEntry($"Bitwise strategy: Completed in {iterations} iterations. " +
            $"Result = {result}, Binary = {Convert.ToString(result, 2)}");

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

