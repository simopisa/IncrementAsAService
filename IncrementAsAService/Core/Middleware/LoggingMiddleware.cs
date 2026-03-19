
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
using IncrementAsAService.Core.Domain.Models;

namespace IncrementAsAService.Core.Middleware;


/// <summary>
/// Logs everything. EVERYTHING. Your future self debugging at 3 AM
/// will thank you. Or curse you. Probably both.
/// </summary>
public sealed class LoggingMiddleware : IIncrementMiddleware<int>
{
    public int Order => 0;

    public async Task<IncrementResult<int>> ExecuteAsync(
        int value, IncrementContext context,
        Func<int, IncrementContext, Task<IncrementResult<int>>> next,
        CancellationToken ct = default)
    {
        var operationId = Guid.NewGuid();
        context.AddAuditEntry($"[LOGGING] ========== INCREMENT OPERATION {operationId} BEGIN ==========");
        context.AddAuditEntry($"[LOGGING] Input value: {value}");
        context.AddAuditEntry($"[LOGGING] Type: {value.GetType().FullName}");
        context.AddAuditEntry($"[LOGGING] Requested by: {context.RequestedBy ?? "Unknown"}");
        context.AddAuditEntry($"[LOGGING] Correlation ID: {context.CorrelationId}");
        context.AddAuditEntry($"[LOGGING] Thread ID: {Environment.CurrentManagedThreadId}");
        context.AddAuditEntry($"[LOGGING] Memory: {GC.GetTotalMemory(false)} bytes");

        var sw = Stopwatch.StartNew();
        var result = await next(value, context);
        sw.Stop();

        context.AddAuditEntry($"[LOGGING] Result: {result.ResultValue}");
        context.AddAuditEntry($"[LOGGING] Success: {result.IsSuccess}");
        context.AddAuditEntry($"[LOGGING] Duration: {sw.Elapsed}");
        context.AddAuditEntry($"[LOGGING] Memory after: {GC.GetTotalMemory(false)} bytes");
        context.AddAuditEntry($"[LOGGING] ========== INCREMENT OPERATION {operationId} END ==========");

        return result;
    }
}

