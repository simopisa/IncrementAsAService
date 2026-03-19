
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
using IncrementAsAService.Core.Domain.Models;

namespace IncrementAsAService.Core.Middleware;


/// <summary>
/// Adds an artificial delay to make the increment feel more "premium."
/// Fast software is suspicious software.
/// </summary>
public sealed class PremiumExperienceMiddleware : IIncrementMiddleware<int>
{
    private readonly int _artificialDelayMs;
    public int Order => 1;

    public PremiumExperienceMiddleware(int artificialDelayMs = 100)
    {
        _artificialDelayMs = artificialDelayMs;
    }

    public async Task<IncrementResult<int>> ExecuteAsync(
        int value, IncrementContext context,
        Func<int, IncrementContext, Task<IncrementResult<int>>> next,
        CancellationToken ct = default)
    {
        context.AddAuditEntry($"[PREMIUM] Adding {_artificialDelayMs}ms artificial delay " +
            "for a more premium user experience...");

        await Task.Delay(_artificialDelayMs, ct);

        context.AddAuditEntry("[PREMIUM] Premium delay complete. " +
            "User now perceives this as a sophisticated operation.");

        return await next(value, context);
    }
}

