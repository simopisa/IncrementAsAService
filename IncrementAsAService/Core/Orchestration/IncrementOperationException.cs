
namespace IncrementAsAService.Core.Orchestration;

public sealed class IncrementOperationException : Exception
{
    public IncrementOperationException(string message) : base(message) { }
    public IncrementOperationException(string message, Exception inner) : base(message, inner) { }
}
