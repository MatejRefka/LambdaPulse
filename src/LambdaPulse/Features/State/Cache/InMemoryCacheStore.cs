using System.Collections.Concurrent;

namespace LambdaPulse.Engine.Features.State.Cache;

internal sealed class InMemoryCacheStore : ICacheStore
{
    //thread-safe against multiple concurrent requests
    private readonly ConcurrentDictionary<string, CachedResponse> _cachedResponses = new();

    public Task<CachedResponse?> GetCachedResponse(string key, CancellationToken cancellationToken = default)
    {
        if (!_cachedResponses.TryGetValue(key, out var cachedResponse))
        {
            return Task.FromResult<CachedResponse?>(null);
        }

        if (cachedResponse.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _cachedResponses.TryRemove(key, out _);
            return Task.FromResult<CachedResponse?>(null);
        }

        return Task.FromResult<CachedResponse?>(cachedResponse);
    }

    public Task<bool> SetCachedResponse(string key, CachedResponse cachedResponse, CancellationToken cancellationToken = default)
    {
        _cachedResponses[key] = cachedResponse;
        return Task.FromResult(true);
    }
}
