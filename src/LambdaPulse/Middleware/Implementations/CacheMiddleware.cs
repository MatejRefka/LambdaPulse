using LambdaPulse.Engine.Features.Logging;
using LambdaPulse.Engine.Features.State.Cache;
using LambdaPulse.Engine.Http.Abstractions;
using LambdaPulse.Engine.Shared.Extensions;

namespace LambdaPulse.Engine.Middleware.Implementations;

/// <summary>
/// Performs a cache lookup for eligible incoming requests. 
/// On a cache hit, the cached response is returned, short-circuiting, saving compute of the endpoint.
/// On a cache miss, the request is processed downstream.
/// Upstream, eligible responses are cached in the cache store.
/// </summary>
internal sealed class CacheMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "Cache";

    private readonly ICacheStore _cacheStore;

    public CacheMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, ICacheStore cacheStore) : base(nextFunction)
    {
        _cacheStore = cacheStore;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var downstreamStart = DateTimeOffset.UtcNow;
        var logs = new List<string>();

        //skip non-get requests
        if (!string.Equals(webContext.WebRequest.Method, "GET", StringComparison.OrdinalIgnoreCase))
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "Request method is not GET. Skip cache lookup." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //skip if no endpoint matched
        if (webContext.Endpoint == null)
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "No endpoint matched the request. Skip cache lookup." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //skip if no cache policy set
        if (webContext.Endpoint.CachePolicy == null)
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "Endpoint has no cache policy. Skip cache lookup." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //skip if cache policy disabled
        if (!webContext.Endpoint.CachePolicy.Enabled)
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "Endpoint cache policy is disabled. Skip cache lookup." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //skip if cache policy duration is 0 or negative
        if (webContext.Endpoint.CachePolicy.DurationSeconds <= 0)
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "Endpoint cache duration is 0 or negative. Skip cache lookup." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //skip if endpoint is not public
        if (!webContext.Endpoint.AllowAnonymous)
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "Endpoint does not allow anonymous access. Skip cache lookup." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //build cache key
        var queryString = webContext.WebRequest.QueryParameters.Count == 0 ? string.Empty : "?" + string.Join("&", webContext.WebRequest.QueryParameters.OrderBy(param => param.Key, StringComparer.OrdinalIgnoreCase).Select(param => $"{param.Key}={param.Value}"));
        var cacheKey = $"{webContext.WebRequest.Method.ToUpperInvariant()}:{webContext.WebRequest.Path}{queryString}";
        logs.Add($"Cache key built. key={cacheKey}.");

        var cachedResponse = await _cacheStore.GetCachedResponse(cacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            webContext.WebResponse.StatusCode = cachedResponse.StatusCode;
            webContext.WebResponse.ResponsePhrase = cachedResponse.ResponsePhrase;
            if (!string.IsNullOrWhiteSpace(cachedResponse.ContentType))
            {
                webContext.WebResponse.Headers["Content-Type"] = cachedResponse.ContentType;
            }
            await webContext.WebResponse.WriteBytesToBody(cachedResponse.Body, cancellationToken);

            logs.Add("Cache hit. Serve cached response.");
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.ShortCircuit, downstreamStart, logs);
            return;
        }

        logs.Add("Cache miss. Continue downstream.");
        RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, logs);

        await _nextFunction(webContext, cancellationToken);

        var upstreamStart = DateTimeOffset.UtcNow;

        //skip caching for malformed response
        if (webContext.WebResponse.StatusCode == null || webContext.WebResponse.ResponsePhrase == null)
        {
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Response is missing status code or reason phrase. Leave response uncached." });
            return;
        }

        //skip caching for non-200 responses
        if (webContext.WebResponse.StatusCode != 200)
        {
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { $"{webContext.WebResponse.StatusCode} response. Leave response uncached." });
            return;
        }

        //skip caching if response body is empty
        if (!webContext.WebResponse.HasBody || webContext.WebResponse.Body.Length == 0)
        {
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Response body is empty. Leave response uncached." });
            return;
        }

        //skip caching if response contains cookies
        if (webContext.WebResponse.Cookies.Count > 0)
        {
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Response contains cookies. Leave response uncached." });
            return;
        }

        //skip caching if response contains Set-Cookie header
        if (webContext.WebResponse.Headers.ContainsKey("Set-Cookie"))
        {
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Response contains Set-Cookie header. Leave response uncached." });
            return;
        }

        //skip caching if response contains X-CSRF-Token header
        if (webContext.WebResponse.Headers.ContainsKey("X-CSRF-Token"))
        {
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Response contains X-CSRF-Token header. Leave response uncached." });
            return;
        }

        //skip caching if Cache-Control header contains no-store or private
        if (webContext.WebResponse.Headers.TryGetValue("Cache-Control", out var value))
        {
            var entries = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (entries.Any(entry => string.Equals(entry, "no-store", StringComparison.OrdinalIgnoreCase)))
            {
                RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Cache-Control contains no-store. Leave response uncached." });
                return;
            }
            if (entries.Any(entry => string.Equals(entry, "private", StringComparison.OrdinalIgnoreCase)))
            {
                RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Cache-Control contains private. Leave response uncached." });
                return;
            }
        }

        var cacheResponse = new CachedResponse
        {
            StatusCode = webContext.WebResponse.StatusCode.Value,
            ResponsePhrase = webContext.WebResponse.ResponsePhrase,
            ContentType = webContext.WebResponse.Headers.TryGetValue("Content-Type", out var contentType) ? contentType : null,
            Body = webContext.WebResponse.Body.ToArray(),
            ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(webContext.Endpoint.CachePolicy.DurationSeconds)
        };

        var isCacheSet = await _cacheStore.SetCachedResponse(cacheKey, cacheResponse, cancellationToken);

        RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, isCacheSet ? new List<string> { "Response is eligible. Cache response." } : new List<string> { "Cache store rejected the write. Leave response uncached." });
    }
}
