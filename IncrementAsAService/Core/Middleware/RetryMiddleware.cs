
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
using IncrementAsAService.Core.Domain.Models;

namespace IncrementAsAService.Core.Middleware;

public sealed class RetryMiddleware : IIncrementMiddleware<int>
{
    public int Order => 2;

    public async Task<IncrementResult<int>> ExecuteAsync(
        int value, IncrementContext context,
        Func<int, IncrementContext, Task<IncrementResult<int>>> next,
        CancellationToken ct = default)
    {
        var maxRetries = context.Options.MaxRetries;
        IncrementResult<int>? lastResult = null;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var delay = context.Options.RetryPolicy switch
                {
                    RetryPolicy.Linear => TimeSpan.FromMilliseconds(100 * attempt),
                    RetryPolicy.ExponentialBackoff => TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 50),
                    RetryPolicy.RandomizedJitter => TimeSpan.FromMilliseconds(new Random().Next(50, 500)),
                    _ => TimeSpan.Zero
                };

                context.AddAuditEntry($"[RETRY] Attempt {attempt + 1}/{maxRetries + 1}. " +
                    $"Waiting {delay.TotalMilliseconds}ms before retry...");
                await Task.Delay(delay, ct);
            }

            lastResult = await next(value, context);
            if (lastResult.IsSuccess) return lastResult;
        }

        return lastResult!;
    }
}


