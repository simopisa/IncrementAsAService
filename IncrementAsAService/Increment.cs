// ╔══════════════════════════════════════════════════════════════════════════════╗
// ║                                                                              ║
// ║   ██╗ █████╗  █████╗ ███████╗    IncrementAsAService™                        ║
// ║   ██║██╔══██╗██╔══██╗██╔════╝    Enterprise Edition v1.0.0-rc3               ║
// ║   ██║███████║███████║███████╗    "Because n++ wasn't scalable enough"        ║
// ║   ██║██╔══██║██╔══██║╚════██║                                                ║
// ║   ██║██║  ██║██║  ██║███████║    Licensed under the WTFPL                    ║
// ║   ╚═╝╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝    (Do What The F*** You Want Public License)  ║
// ║                                                                              ║
// ║   WARNING: This library is enterprise-grade software. Any attempt to         ║
// ║   simply write "n + 1" will be met with a code review rejection              ║
// ║                                                                              ║
// ╚══════════════════════════════════════════════════════════════════════════════╝

using IncrementAsAService.Configuration;
using IncrementAsAService.Core.Domain.Models;
using IncrementAsAService.Core.Orchestration;

namespace IncrementAsAService;

/// <summary>
/// The simple entry point. 
/// 
/// Usage:
///     var result = await Increment.ThisNumber(41)
///         .BecauseINeedTo("calculate the meaning of life + 1")
///         .PleaseAsync();
///         
/// Or for the impatient:
///     var result = await Increment.JustDoIt(41);
/// </summary>
public static class Increment
{
    private static readonly Lazy<IncrementOrchestrator> _defaultOrchestrator = new(() =>
        IncrementOrchestratorBuilder.Create()
            .WithFullEnterpriseConfiguration()
            .Build());

    // ── Fluent API ────────────────────────────────────────────

    public static IncrementRequestBuilder ThisNumber(int value) => new(value);

    /// <summary>
    /// For when you don't have time for fluent APIs and just need n+1.
    /// Still goes through the full enterprise pipeline though. 
    /// Standards are standards.
    /// </summary>
    public static async Task<IncrementResult<int>> JustDoIt(int value)
    {
        var request = IncrementRequest<int>.Create(
            value,
            justification: "User invoked JustDoIt(). No justification provided. Suspicious.");
        return await _defaultOrchestrator.Value.OrchestrateAsync(request);
    }

    /// <summary>
    /// Increments multiple numbers because apparently one at a time
    /// wasn't fast enough.
    /// </summary>
    public static async Task<BatchIncrementResult<int>> ThoseNumbers(params int[] values)
    {
        var requests = values.Select(v => IncrementRequest<int>.Create(v));
        return await _defaultOrchestrator.Value.OrchestrateBatchAsync(
            requests, new BatchOptions());
    }

    public sealed class IncrementRequestBuilder
    {
        private readonly int _value;
        private string? _requestedBy;
        private string? _justification;
        private Priority _priority = Priority.Normal;
        private IncrementOptions _options = new();

        internal IncrementRequestBuilder(int value) => _value = value;

        public IncrementRequestBuilder AsUser(string user)
        { _requestedBy = user; return this; }

        public IncrementRequestBuilder BecauseINeedTo(string justification)
        { _justification = justification; return this; }

        public IncrementRequestBuilder WithPriority(Priority priority)
        { _priority = priority; return this; }

        public IncrementRequestBuilder WithOptions(Action<IncrementOptions> configure)
        {
            var options = new IncrementOptions();
            configure(options);
            _options = options;
            return this;
        }

        public IncrementRequestBuilder UsingStrategy(string strategyName)
        {
            _options = new IncrementOptions { PreferredStrategy = strategyName };
            return this;
        }

        public IncrementRequestBuilder Urgently()
        { _priority = Priority.Critical; return this; }

        public IncrementRequestBuilder WheneverYouGetToIt()
        { _priority = Priority.WheneverYouGetToIt; return this; }

        /// <summary>
        /// Actually performs the increment. We say "please" because 
        /// good manners matter, even in code.
        /// </summary>
        public async Task<IncrementResult<int>> PleaseAsync(
            CancellationToken cancellationToken = default)
        {
            var request = new IncrementRequest<int>
            {
                Value = _value,
                RequestedBy = _requestedBy ?? Environment.UserName,
                Justification = _justification ?? "No justification provided",
                Priority = _priority,
                Options = _options
            };

            return await _defaultOrchestrator.Value.OrchestrateAsync(
                request, cancellationToken);
        }
    }
}
