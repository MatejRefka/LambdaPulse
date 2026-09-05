namespace LambdaPulse.Features.State.Cache;

/// <summary>
/// Endpoint's cache policy.
/// </summary>
public sealed class CachePolicy
{
    /// <summary>
    /// Indicates whether caching is enabled for the endpoint.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Duration in seconds for which the response should be cached.
    /// Default to 60 seconds if no duration is specified.
    /// </summary>
    public int DurationSeconds { get; init; } = 60;
}
