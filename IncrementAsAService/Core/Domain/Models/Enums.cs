
using IncrementAsAService.Core.Abstractions.Interfaces.Contracts;
namespace IncrementAsAService.Core.Domain.Models;

/// <summary>
/// How confident are we that n+1 is correct?
/// You'd think it's always "Absolute" but you'd be wrong.
/// </summary>
public enum ConfidenceLevel
{
    /// <summary>We're not even sure what a number is anymore</summary>
    Existential = 0,
    /// <summary>The result might be correct. Or not. Who's to say?</summary>
    Low = 25,
    /// <summary>Probably right but we should schedule a meeting to discuss</summary>
    Medium = 50,
    /// <summary>We're fairly confident. The unit tests pass at least</summary>
    High = 75,
    /// <summary>It's n+1. We're sure. Almost. Like 99% sure.</summary>
    VeryHigh = 90,
    /// <summary>Mathematically proven. Peer reviewed. Published in Nature.</summary>
    Absolute = 100
}


public enum Priority { Critical = 0, High = 1, Normal = 2, Low = 3, WheneverYouGetToIt = 4 }

public enum RetryPolicy
{
    None,
    Linear,
    ExponentialBackoff,
    /// <summary>Retry, but wait a random amount of time. For chaos engineering.</summary>
    RandomizedJitter,
    /// <summary>Just keep trying forever. YOLO.</summary>
    Infinite
}




