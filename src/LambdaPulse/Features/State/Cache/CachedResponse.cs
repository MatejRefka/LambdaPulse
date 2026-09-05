namespace LambdaPulse.Features.State.Cache;

/// <summary>
/// Cached HTTP response.
/// </summary>
public sealed class CachedResponse
{
    /// <summary>
    /// Response status code.
    /// </summary>
    public required int StatusCode { get; init; }

    /// <summary>
    /// Response phrase.
    /// </summary>
    public required string ResponsePhrase { get; init; }

    /// <summary>
    /// Response body.
    /// </summary>
    public required byte[] Body { get; init; }

    /// <summary>
    /// Response content type.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Response expiration date and time.
    /// </summary>
    public required DateTimeOffset ExpiresAt { get; init; }
}
