
using IncrementAsAService.Core.Domain.Models;
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
namespace IncrementAsAService.Core.Strategies;



/// <summary>
/// The "Monte Carlo" approach: generates random numbers until one of them
/// happens to be n+1. Statistically guaranteed to finish. Eventually.
/// </summary>
public sealed class MonteCarloIncrementStrategy : IIncrementStrategy<int>
{
    public string StrategyName => "MonteCarlo";
    public string StrategyDescription =>
        "Generates random numbers until it finds n+1. It's not stupid, it's stochastic.";
    public Version StrategyVersion => new(1, 0, 0);
    public int Priority => 1; // Low priority.

    public bool CanHandle(int value) => value >= 0 && value < 100; // Let's not get crazy

    public Task<IncrementResult<int>> IncrementAsync(
        int value, IncrementContext context, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var target = value + 1; // Yes, we know the answer. It's all about the journey.
        var rng = new Random();
        int attempts = 0;
        int guess;

        context.AddAuditEntry($"MonteCarlo strategy: Beginning stochastic search for S({value})");
        context.AddAuditEntry($"MonteCarlo strategy: Search space = [0, {value + 100}]");

        do
        {
            attempts++;
            guess = rng.Next(0, value + 100);

            if (attempts % 100 == 0)
                context.AddAuditEntry($"MonteCarlo strategy: {attempts} attempts so far... " +
                    $"Last guess: {guess}. Still searching.");

            if (ct.IsCancellationRequested)
            {
                return Task.FromResult(new IncrementResult<int>
                {
                    OriginalValue = value,
                    ResultValue = value,
                    IsSuccess = false,
                    ErrorMessage = $"Stochastic search cancelled after {attempts} attempts. " +
                        "The universe is not random enough today.",
                    StrategyUsed = StrategyName,
                    Duration = sw.Elapsed,
                    RetryCount = attempts
                });
            }
        } while (guess != target);

        sw.Stop();
        context.AddAuditEntry($"MonteCarlo strategy: EUREKA! Found {target} after {attempts} " +
            $"random attempts in {sw.Elapsed}!");

        return Task.FromResult(new IncrementResult<int>
        {
            OriginalValue = value,
            ResultValue = guess,
            IsSuccess = true,
            StrategyUsed = StrategyName,
            Duration = sw.Elapsed,
            RetryCount = attempts,
            Confidence = ConfidenceLevel.VeryHigh, // I checked, it really is n+1
            AuditTrail = context.AuditEntries.ToList()
        });
    }
}

