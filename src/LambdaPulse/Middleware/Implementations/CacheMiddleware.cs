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
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "Non-GET request. Cache skipped." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //skip if no endpoint matched
        if (webContext.Endpoint == null)
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "No endpoint matched. Cache skipped." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //skip if no cache policy set
        if (webContext.Endpoint.CachePolicy == null)
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "No cache policy set. Cache skipped." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //skip if cache policy disabled
        if (!webContext.Endpoint.CachePolicy.Enabled)
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "Cache policy disabled. Cache skipped." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //skip if cache policy duration is 0 or negative
        if (webContext.Endpoint.CachePolicy.DurationSeconds <= 0)
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "Cache policy duration is 0 or negative. Cache skipped." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //skip if endpoint is not public
        if (!webContext.Endpoint.AllowAnonymous)
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "Endpoint does not allow anonymous access. Cache skipped." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //build cache key
        var queryString = webContext.WebRequest.QueryParameters.Count == 0 ? string.Empty : "?" + string.Join("&", webContext.WebRequest.QueryParameters.OrderBy(param => param.Key, StringComparer.OrdinalIgnoreCase).Select(param => $"{param.Key}={param.Value}"));
        var cacheKey = $"{webContext.WebRequest.Method.ToUpperInvariant()}:{webContext.WebRequest.Path}{queryString}";
        logs.Add($"Constructed cache key: {cacheKey}");

        var cachedResponse = await _cacheStore.GetCachedResponse(cacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            webContext.WebResponse.StatusCode = cachedResponse.StatusCode;
            webContext.WebResponse.ResponsePhrase = cachedResponse.ResponsePhrase;
            //separate dictionary reference for response and cache
            webContext.WebResponse.Headers = new Dictionary<string, string>(cachedResponse.Headers, StringComparer.OrdinalIgnoreCase);
            await webContext.WebResponse.WriteBytesToBody(cachedResponse.Body, cancellationToken);

            logs.Add("Cache hit. Returning cached response.");
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.ShortCircuit, downstreamStart, logs);
            return;
        }

        logs.Add("Cache miss. Continuing downstream.");
        RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, logs);

        await _nextFunction(webContext, cancellationToken);

        var upstreamStart = DateTimeOffset.UtcNow;

        //skip caching for malformed response
        if (webContext.WebResponse.StatusCode == null || webContext.WebResponse.ResponsePhrase == null)
        {
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Response missing status code or response phrase. Cannot cache response." });
            return;
        }

        //skip caching for non-200 responses
        if (webContext.WebResponse.StatusCode != 200)
        {
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { $"{webContext.WebResponse.StatusCode} response. Cannot cache response." });
            return;
        }

        //skip caching if response body is empty
        if (!webContext.WebResponse.HasBody || webContext.WebResponse.Body.Length == 0)
        {
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Response body is empty. Cannot cache response." });
            return;
        }

        //skip caching if response contains cookies
        if (webContext.WebResponse.Cookies.Count > 0)
        {
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Response contains cookies. Cannot cache response." });
            return;
        }

        //skip caching if response contains Set-Cookie header
        if (webContext.WebResponse.Headers.ContainsKey("Set-Cookie"))
        {
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Response contains 'Set-Cookie' header. Cannot cache response." });
            return;
        }

        //skip caching if response contains X-CSRF-Token header
        if (webContext.WebResponse.Headers.ContainsKey("X-CSRF-Token"))
        {
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Response contains 'X-CSRF-Token' header. Cannot cache response." });
            return;
        }

        //skip caching if Cache-Control header contains no-store or private
        if (webContext.WebResponse.Headers.TryGetValue("Cache-Control", out var value))
        {
            var entries = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (entries.Any(entry => string.Equals(entry, "no-store", StringComparison.OrdinalIgnoreCase)))
            {
                RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Response contains 'Cache-Control: no-store'. Cannot cache response." });
                return;
            }
            if (entries.Any(entry => string.Equals(entry, "private", StringComparison.OrdinalIgnoreCase)))
            {
                RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Response contains 'Cache-Control: private'. Cannot cache response." });
                return;
            }
        }

        var cacheResponse = new CachedResponse
        {
            StatusCode = webContext.WebResponse.StatusCode.Value,
            ResponsePhrase = webContext.WebResponse.ResponsePhrase,
            //separate dictionary reference for cache and response
            Headers = new Dictionary<string, string>(webContext.WebResponse.Headers, StringComparer.OrdinalIgnoreCase),
            Body = webContext.WebResponse.Body.ToArray(),
            ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(webContext.Endpoint.CachePolicy.DurationSeconds)
        };

        await _cacheStore.SetCachedResponse(cacheKey, cacheResponse, cancellationToken);

        RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Response cached." });
    }
}
