namespace LambdaPulse.Features.State.Cache;

/// <summary>
/// Defines operations for retrieving and saving cached responses
/// </summary>
public interface ICacheStore
{
    /// <summary>
    /// Retrieves the cached response for the given cache key
    /// </summary>
    Task<CachedResponse?> GetCachedResponse(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the given cached response
    /// </summary>
    Task SaveCachedResponse(string key, CachedResponse cachedResponse, CancellationToken cancellationToken = default);
}
