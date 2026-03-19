



using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;

using IncrementAsAService.Core.Domain.Models;
namespace IncrementAsAService.Core.Validation;

/// <summary>
/// Rate limiter. Because you can't just go around incrementing numbers
/// There are LIMITS.
/// </summary>
public sealed class RateLimitValidator : IIncrementValidator<int>
{
    private readonly ConcurrentDictionary<string, Queue<DateTimeOffset>> _requestHistory = new();
    private readonly int _maxRequestsPerMinute;
    public string ValidatorName => "RateLimiter";

    public RateLimitValidator(int maxRequestsPerMinute = 60)
    {
        _maxRequestsPerMinute = maxRequestsPerMinute;
    }

    public Task<ValidationResult> ValidateAsync(
        int value, IncrementContext context, CancellationToken ct = default)
    {
        var key = context.RequestedBy ?? "anonymous";
        var now = DateTimeOffset.UtcNow;
        var window = now.AddMinutes(-1);

        var queue = _requestHistory.GetOrAdd(key, _ => new Queue<DateTimeOffset>());
        lock (queue)
        {
            while (queue.Count > 0 && queue.Peek() < window) queue.Dequeue();

            if (queue.Count >= _maxRequestsPerMinute)
            {
                context.AddAuditEntry($"RateLimiter: User '{key}' has exceeded " +
                    $"{_maxRequestsPerMinute} increments/minute. Slow down.");
                return Task.FromResult(ValidationResult.Failure(ValidatorName,
                    $"Rate limit exceeded. You've incremented too many numbers this minute. " +
                    $"Please wait and reflect on whether you really need all those +1s."));
            }

            queue.Enqueue(now);
        }

        context.AddAuditEntry($"RateLimiter: User '{key}' — {queue.Count}/{_maxRequestsPerMinute} " +
            "requests this minute. ✓");
        return Task.FromResult(ValidationResult.Success(ValidatorName));
    }
}
