
namespace IncrementAsAService.Core.Abstractions.Interfaces.Contracts;

/// <summary>
/// Telemetry for monitoring your increment operations in production.
/// Because if your n+1 goes down at 3 AM, you need to know.
/// </summary>
public interface IIncrementTelemetry
{
    void RecordIncrementAttempt(string strategyName);
    void RecordIncrementSuccess(string strategyName, TimeSpan duration);
    void RecordIncrementFailure(string strategyName, Exception exception);
    void RecordCacheHit();
    void RecordCacheMiss();
    TelemetrySnapshot GetSnapshot();
}
