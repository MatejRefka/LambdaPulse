namespace LambdaPulse.Features.State.Cache;

public interface ICacheStore
{
    Task<CachedResponse?> GetCachedResponse(string key, CancellationToken cancellationToken = default);

    Task<bool> SetCachedResponse(string key, CachedResponse cachedResponse, CancellationToken cancellationToken = default);
}
