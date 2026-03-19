
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
using IncrementAsAService.Core.Domain.Models;
using IncrementAsAService.Core.EventSourcing;
using IncrementAsAService.Core.Infrastructure;
using IncrementAsAService.Core.Middleware;
using IncrementAsAService.Core.Strategies;
using IncrementAsAService.Core.Validation;

namespace IncrementAsAService.Core.Orchestration;

/// <summary>
/// The IncrementOrchestrator: a symphony conductor for the world's
/// simplest operation. 
/// 
/// It coordinates validators, strategies, middleware, caching, 
/// telemetry, event sourcing, and conflict resolution to ultimately
/// compute: n + 1.
/// </summary>
public sealed class IncrementOrchestrator : IIncrementOrchestrator<int>
{
    private readonly IReadOnlyList<IIncrementStrategy<int>> _strategies;
    private readonly IReadOnlyList<IIncrementValidator<int>> _validators;
    private readonly IReadOnlyList<IIncrementMiddleware<int>> _middleware;
    private readonly IReadOnlyList<IIncrementObserver> _observers;
    private readonly IIncrementCache<int> _cache;
    private readonly IIncrementTelemetry _telemetry;
    private readonly IncrementEventStore _eventStore;

    public IncrementOrchestrator(
        IEnumerable<IIncrementStrategy<int>>? strategies = null,
        IEnumerable<IIncrementValidator<int>>? validators = null,
        IEnumerable<IIncrementMiddleware<int>>? middleware = null,
        IEnumerable<IIncrementObserver>? observers = null,
        IIncrementCache<int>? cache = null,
        IIncrementTelemetry? telemetry = null,
        IncrementEventStore? eventStore = null)
    {
        _strategies = (strategies ?? DefaultStrategies()).OrderByDescending(s => s.Priority).ToList();
        _validators = (validators ?? DefaultValidators()).ToList();
        _middleware = (middleware ?? DefaultMiddleware()).OrderBy(m => m.Order).ToList();
        _observers = (observers ?? Array.Empty<IIncrementObserver>()).ToList();
        _cache = cache ?? new InMemoryIncrementCache();
        _telemetry = telemetry ?? new IncrementTelemetryCollector();
        _eventStore = eventStore ?? new IncrementEventStore();
    }

    public async Task<IncrementResult<int>> OrchestrateAsync(
        IncrementRequest<int> request,
        CancellationToken cancellationToken = default)
    {
        var context = new IncrementContext
        {
            CorrelationId = request.RequestId,
            RequestedBy = request.RequestedBy,
            Options = request.Options
        };

        context.AddAuditEntry($"Orchestrator: Received increment request for value {request.Value}");
        context.AddAuditEntry($"Orchestrator: Request ID = {request.RequestId}");
        context.AddAuditEntry($"Orchestrator: Priority = {request.Priority}");
        context.AddAuditEntry($"Orchestrator: Justification = \"{request.Justification}\"");

        _eventStore.Append(request.RequestId,
            new IncrementRequestedEvent<int>(request.Value, request.RequestedBy));

        // Notify observers
        var @event = new IncrementEvent
        {
            EventType = "BeforeIncrement",
            OriginalValue = request.Value
        };
        foreach (var observer in _observers)
            await observer.OnBeforeIncrementAsync(@event);

        try
        {
            // Check cache
            if (request.Options.EnableCaching)
            {
                context.AddAuditEntry("Orchestrator: Phase 1 — Checking cache...");
                var cached = await _cache.GetCachedResultAsync(request.Value);
                if (cached is not null)
                {
                    _telemetry.RecordCacheHit();
                    _eventStore.Append(request.RequestId,
                        new IncrementCacheHitEvent<int>(request.Value, cached.ResultValue));
                    context.AddAuditEntry($"Orchestrator: CACHE HIT! {request.Value} → {cached.ResultValue}");
                    return cached;
                }
                _telemetry.RecordCacheMiss();
                context.AddAuditEntry("Orchestrator: Cache miss. Proceeding with full pipeline.");
            }

            // Validation
            if (request.Options.RunValidation)
            {
                context.AddAuditEntry("Orchestrator: Phase 2 — Running validation pipeline...");
                foreach (var validator in _validators)
                {
                    var validationResult = await validator.ValidateAsync(
                        request.Value, context, cancellationToken);

                    if (!validationResult.IsValid)
                    {
                        var failure = $"Validation failed ({validator.ValidatorName}): " +
                            string.Join("; ", validationResult.Errors);
                        context.AddAuditEntry($"Orchestrator: ❌ {failure}");

                        _eventStore.Append(request.RequestId,
                            new IncrementValidationFailedEvent(validator.ValidatorName, failure));

                        return new IncrementResult<int>
                        {
                            OriginalValue = request.Value,
                            ResultValue = request.Value,
                            IsSuccess = false,
                            ErrorMessage = failure,
                            StrategyUsed = "None — validation failed",
                            AuditTrail = context.AuditEntries.ToList()
                        };
                    }

                    _eventStore.Append(request.RequestId,
                        new IncrementValidationPassedEvent(validator.ValidatorName));
                    context.AddAuditEntry($"Orchestrator: ✓ {validator.ValidatorName} passed");

                    foreach (var warning in validationResult.Warnings)
                        context.AddAuditEntry($"Orchestrator: [WARNING] Warning from {validator.ValidatorName}: {warning}");
                }
            }

            // Strategy selection
            context.AddAuditEntry("Orchestrator: Phase 3 — Selecting increment strategy...");
            var selectedStrategy = _strategies.FirstOrDefault(s => s.CanHandle(request.Value));

            if (request.Options.PreferredStrategy is not null)
            {
                var preferred = _strategies.FirstOrDefault(s =>
                    s.StrategyName.Equals(request.Options.PreferredStrategy,
                        StringComparison.OrdinalIgnoreCase) && s.CanHandle(request.Value));

                if (preferred is not null)
                {
                    selectedStrategy = preferred;
                    context.AddAuditEntry($"Orchestrator: Using preferred strategy: {preferred.StrategyName}");
                }
                else
                {
                    context.AddAuditEntry($"Orchestrator: Preferred strategy " +
                        $"'{request.Options.PreferredStrategy}' not available. Using default.");
                }
            }

            if (selectedStrategy is null)
            {
                return new IncrementResult<int>
                {
                    OriginalValue = request.Value,
                    ResultValue = request.Value,
                    IsSuccess = false,
                    ErrorMessage = "No strategy available to increment this value. " +
                        "The number is beyond our capabilities. We are sorry.",
                    StrategyUsed = "None"
                };
            }

            _eventStore.Append(request.RequestId,
                new IncrementStrategySelectedEvent(selectedStrategy.StrategyName));
            context.AddAuditEntry($"Orchestrator: Selected strategy: {selectedStrategy.StrategyName} " +
                $"(v{selectedStrategy.StrategyVersion}, priority={selectedStrategy.Priority})");
            context.AddAuditEntry($"Orchestrator: Strategy description: {selectedStrategy.StrategyDescription}");

            // Execute through middleware pipeline
            context.AddAuditEntry("Orchestrator: Phase 4 — Executing through middleware pipeline...");
            _telemetry.RecordIncrementAttempt(selectedStrategy.StrategyName);

            Func<int, IncrementContext, Task<IncrementResult<int>>> core =
                (v, ctx) => selectedStrategy.IncrementAsync(v, ctx, cancellationToken);

            // Build middleware chain (in reverse order)
            var pipeline = _middleware.Reverse().Aggregate(core,
                (next, mw) => (v, ctx) => mw.ExecuteAsync(v, ctx, next, cancellationToken));

            var result = await pipeline(request.Value, context);

            if (result.IsSuccess)
            {
                _telemetry.RecordIncrementSuccess(selectedStrategy.StrategyName, result.Duration);
                _eventStore.Append(request.RequestId,
                    new IncrementExecutedEvent<int>(request.Value, result.ResultValue, result.Duration));

                // Cache the result
                if (request.Options.EnableCaching)
                {
                    await _cache.SetCachedResultAsync(request.Value, result);
                    context.AddAuditEntry("Orchestrator: Result cached for future use.");
                }

                // Notify observers
                var afterEvent = new IncrementEvent
                {
                    EventType = "AfterIncrement",
                    OriginalValue = request.Value,
                    ResultValue = result.ResultValue,
                    StrategyUsed = selectedStrategy.StrategyName
                };
                foreach (var observer in _observers)
                    await observer.OnAfterIncrementAsync(afterEvent);
            }
            else
            {
                _telemetry.RecordIncrementFailure(selectedStrategy.StrategyName,
                    new Exception(result.ErrorMessage));
                _eventStore.Append(request.RequestId,
                    new IncrementFailedEvent(result.ErrorMessage ?? "Unknown", null));
            }

            context.AddAuditEntry($"Orchestrator: Operation complete. " +
                $"{request.Value} → {result.ResultValue}");
            return result with { AuditTrail = context.AuditEntries.ToList() };
        }
        catch (Exception ex)
        {
            _eventStore.Append(request.RequestId,
                new IncrementFailedEvent(ex.Message, ex.GetType().Name));

            foreach (var observer in _observers)
                await observer.OnIncrementFailedAsync(@event, ex);

            throw new IncrementOperationException(
                $"Catastrophic failure while adding 1 to {request.Value}. " +
                $"This should never happen but here we are.", ex);
        }
    }

    public async Task<BatchIncrementResult<int>> OrchestrateBatchAsync(
        IEnumerable<IncrementRequest<int>> requests,
        BatchOptions options,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var semaphore = new SemaphoreSlim(options.MaxDegreeOfParallelism);
        var results = new ConcurrentBag<IncrementResult<int>>();

        var tasks = requests.Select(async request =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var result = await OrchestrateAsync(request, cancellationToken);
                results.Add(result);

                if (!result.IsSuccess && options.StopOnFirstFailure)
                    throw new IncrementOperationException(
                        $"Batch halted: failed to increment {request.Value}");
            }
            finally { semaphore.Release(); }
        });

        await Task.WhenAll(tasks);
        sw.Stop();

        return new BatchIncrementResult<int>
        {
            Results = results.ToList(),
            TotalDuration = sw.Elapsed
        };
    }

    public async Task<IncrementResult<int>> OrchestrateWithRollbackAsync(
        IncrementRequest<int> request,
        CancellationToken cancellationToken = default)
    {
        var result = await OrchestrateAsync(request, cancellationToken);

        if (!result.IsSuccess) return result;

        // Store rollback info
        var rollbackStrategy = _strategies
            .OfType<IReversibleIncrementStrategy<int>>()
            .FirstOrDefault();

        if (rollbackStrategy is not null)
        {
            result = result with
            {
                AuditTrail = result.AuditTrail
                    .Append($"Rollback available via {rollbackStrategy.StrategyName}")
                    .ToList()
            };
        }

        return result;
    }

    // ── Defaults ──────────────────────────────────────────────

    private static IEnumerable<IIncrementStrategy<int>> DefaultStrategies() => new IIncrementStrategy<int>[]
    {
            new ClassicIncrementStrategy(),
            new BitwiseIncrementStrategy(),
            new DoubleNegationIncrementStrategy(),
            new LookupTableIncrementStrategy(),
            new PeanoAxiomIncrementStrategy(),
            new MonteCarloIncrementStrategy()
    };

    private static IEnumerable<IIncrementValidator<int>> DefaultValidators() => new IIncrementValidator<int>[]
    {
            new OverflowValidator(),
            new NegativityValidator(),
            new SuperstitionValidator(),
            new RateLimitValidator()
    };

    private static IEnumerable<IIncrementMiddleware<int>> DefaultMiddleware() => new IIncrementMiddleware<int>[]
    {
            new LoggingMiddleware(),
            new PremiumExperienceMiddleware(artificialDelayMs: 50),
            new RetryMiddleware()
    };

    // ── Diagnostics ───────────────────────────────────────────

    public TelemetrySnapshot GetTelemetry() => _telemetry.GetSnapshot();
    public CacheStatistics GetCacheStatistics() => _cache.GetStatistics();
    public IncrementEventStore GetEventStore() => _eventStore;

 }

