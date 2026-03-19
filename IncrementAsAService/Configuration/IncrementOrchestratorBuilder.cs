namespace IncrementAsAService.Configuration;

using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
using IncrementAsAService.Core.Domain.Models;
using IncrementAsAService.Core.EventSourcing;
using IncrementAsAService.Core.Infrastructure;
using IncrementAsAService.Core.Middleware;
using IncrementAsAService.Core.Orchestration;
using IncrementAsAService.Core.Strategies;
using IncrementAsAService.Core.Validation;


/// <summary>
/// Fluent builder for the IncrementOrchestrator.
/// Because even configuring how to add 1 should feel like poetry.
/// </summary>
public sealed class IncrementOrchestratorBuilder
{
    private readonly List<IIncrementStrategy<int>> _strategies = new();
    private readonly List<IIncrementValidator<int>> _validators = new();
    private readonly List<IIncrementMiddleware<int>> _middleware = new();
    private readonly List<IIncrementObserver> _observers = new();
    private IIncrementCache<int>? _cache;
    private IIncrementTelemetry? _telemetry;
    private IncrementEventStore? _eventStore;

    public static IncrementOrchestratorBuilder Create() => new();

    public IncrementOrchestratorBuilder WithStrategy(IIncrementStrategy<int> strategy)
    { _strategies.Add(strategy); return this; }

    public IncrementOrchestratorBuilder WithClassicStrategy()
    { _strategies.Add(new ClassicIncrementStrategy()); return this; }

    public IncrementOrchestratorBuilder WithBitwiseStrategy()
    { _strategies.Add(new BitwiseIncrementStrategy()); return this; }

    public IncrementOrchestratorBuilder WithDoubleNegationStrategy()
    { _strategies.Add(new DoubleNegationIncrementStrategy()); return this; }

    public IncrementOrchestratorBuilder WithPeanoAxiomStrategy()
    { _strategies.Add(new PeanoAxiomIncrementStrategy()); return this; }

    public IncrementOrchestratorBuilder WithLookupTableStrategy()
    { _strategies.Add(new LookupTableIncrementStrategy()); return this; }

    public IncrementOrchestratorBuilder WithMonteCarloStrategy()
    { _strategies.Add(new MonteCarloIncrementStrategy()); return this; }

    public IncrementOrchestratorBuilder WithAllStrategies()
    {
        return WithClassicStrategy()
            .WithBitwiseStrategy()
            .WithDoubleNegationStrategy()
            .WithPeanoAxiomStrategy()
            .WithLookupTableStrategy()
            .WithMonteCarloStrategy();
    }

    public IncrementOrchestratorBuilder WithValidator(IIncrementValidator<int> validator)
    { _validators.Add(validator); return this; }

    public IncrementOrchestratorBuilder WithOverflowProtection()
    { _validators.Add(new OverflowValidator()); return this; }

    public IncrementOrchestratorBuilder WithNegativityProtection(int minimum = int.MinValue)
    { _validators.Add(new NegativityValidator(minimum)); return this; }

    public IncrementOrchestratorBuilder WithSuperstitionProtection()
    { _validators.Add(new SuperstitionValidator()); return this; }

    public IncrementOrchestratorBuilder WithRateLimiting(int maxPerMinute = 60)
    { _validators.Add(new RateLimitValidator(maxPerMinute)); return this; }

    public IncrementOrchestratorBuilder WithAllValidators()
    {
        return WithOverflowProtection()
            .WithNegativityProtection()
            .WithSuperstitionProtection()
            .WithRateLimiting();
    }

    public IncrementOrchestratorBuilder WithMiddleware(IIncrementMiddleware<int> middleware)
    { _middleware.Add(middleware); return this; }

    public IncrementOrchestratorBuilder WithLogging()
    { _middleware.Add(new LoggingMiddleware()); return this; }

    public IncrementOrchestratorBuilder WithPremiumExperience(int delayMs = 100)
    { _middleware.Add(new PremiumExperienceMiddleware(delayMs)); return this; }

    public IncrementOrchestratorBuilder WithRetryPolicy()
    { _middleware.Add(new RetryMiddleware()); return this; }

    public IncrementOrchestratorBuilder WithCaching(int maxSize = 10000)
    { _cache = new InMemoryIncrementCache(maxSize); return this; }

    public IncrementOrchestratorBuilder WithTelemetry()
    { _telemetry = new IncrementTelemetryCollector(); return this; }

    public IncrementOrchestratorBuilder WithEventSourcing()
    { _eventStore = new IncrementEventStore(); return this; }

    public IncrementOrchestratorBuilder WithObserver(IIncrementObserver observer)
    { _observers.Add(observer); return this; }

    /// <summary>
    /// Configures EVERYTHING. Maximum enterprise. No compromises.
    /// Your architect will weep tears of joy (or despair).
    /// </summary>
    public IncrementOrchestratorBuilder WithFullEnterpriseConfiguration()
    {
        return WithAllStrategies()
            .WithAllValidators()
            .WithLogging()
            .WithPremiumExperience()
            .WithRetryPolicy()
            .WithCaching()
            .WithTelemetry()
            .WithEventSourcing();
    }

    public IncrementOrchestrator Build() => new(
        _strategies.Any() ? _strategies : null,
        _validators.Any() ? _validators : null,
        _middleware.Any() ? _middleware : null,
        _observers.Any() ? _observers : null,
        _cache, _telemetry, _eventStore);
}
