namespace LambdaPulse.Features.State.Cache;

public interface ICacheStore
{
    Task<CachedResponse?> GetCachedResponse(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the given cached response
    /// </summary>
    Task<bool> SaveCachedResponse(string key, CachedResponse cachedResponse, CancellationToken cancellationToken = default);
}
