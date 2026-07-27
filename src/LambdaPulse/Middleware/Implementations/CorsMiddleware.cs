using LambdaPulse.Configuration;
using LambdaPulse.Features.Logging;
using LambdaPulse.Http.Abstractions;
using LambdaPulse.Shared.Extensions;
using System.Globalization;

namespace LambdaPulse.Middleware.Implementations;

/// <summary>
/// Inspects the origin header of incoming requests and adds CORS headers based on the server's configuration.
/// These override default same-origin policy browser behavior and allow or restrict cross-origin requests.
/// Response headers pertain to origins, methods, headers, exposed headers, and credentials allowed by the server.
/// </summary>
internal sealed class CorsMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "CORS";

    private readonly HashSet<string> _allowedOrigins;
    private readonly bool _allowCredentials;
    private readonly HashSet<string> _exposedHeaders;
    private readonly HashSet<string> _allowedMethods;
    private readonly HashSet<string> _allowedHeaders;
    private readonly int _preflightMaxAgeSeconds;
    public CorsMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, IConfigProvider configProvider) : base(nextFunction)
    {
        _allowedOrigins = configProvider.ServerConfig.MiddlewareConfig.CorsMiddleware.AllowedOrigins;
        _allowCredentials = configProvider.ServerConfig.MiddlewareConfig.CorsMiddleware.AllowCredentials;
        _exposedHeaders = configProvider.ServerConfig.MiddlewareConfig.CorsMiddleware.ExposedHeaders;
        _allowedMethods = configProvider.ServerConfig.MiddlewareConfig.CorsMiddleware.AllowedMethods;
        _allowedHeaders = configProvider.ServerConfig.MiddlewareConfig.CorsMiddleware.AllowedHeaders;
        _preflightMaxAgeSeconds = configProvider.ServerConfig.MiddlewareConfig.CorsMiddleware.PreflightMaxAgeSeconds;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var downstreamStart = DateTimeOffset.UtcNow;
        var logs = new List<string>();

        webContext.WebRequest.Headers.TryGetValue("Origin", out var origin);

        //same-origin or non-browser request
        if (string.IsNullOrWhiteSpace(origin))
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "Origin header is missing. Skip CORS." });
            await _nextFunction(webContext, cancellationToken);

            var deafultUpstreamStart = DateTimeOffset.UtcNow;

            //add "Origin" to Vary header. External cache now needs to check request Origin before serving cached content
            webContext.WebResponse.ApplyVaryHeader("Origin");

            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, deafultUpstreamStart, new List<string> { "Append Origin to Vary header for external caching." });
            return;
        }

        //origin is not within the allowed list
        if (!_allowedOrigins.Contains(origin))
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "Origin is not allowed. Skip CORS." });
            await _nextFunction(webContext, cancellationToken);
            var disallowedOriginUpstreamStart = DateTimeOffset.UtcNow;

            //add "Origin" to Vary header. Cache now needs to check request Origin before serving cached content
            webContext.WebResponse.ApplyVaryHeader("Origin");

            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, disallowedOriginUpstreamStart, new List<string> { "Append Origin to Vary header for external caching." });
            return;
        }

        //preflight request
        if (string.Equals(webContext.WebRequest.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase) && webContext.WebRequest.Headers.ContainsKey("Access-Control-Request-Method"))
        {
            //add "Origin" to Vary header. Cache now needs to check request Origin before serving cached content
            webContext.WebResponse.ApplyVaryHeader("Origin");
            webContext.WebResponse.ApplyVaryHeader("Access-Control-Request-Method");
            webContext.WebResponse.ApplyVaryHeader("Access-Control-Request-Headers");
            logs.Add("Append preflight options to Vary header for external caching.");

            webContext.WebResponse.Headers["Access-Control-Allow-Origin"] = origin;
            logs.Add($"Allowed origin is set. origin={origin}.");

            //methods that are allowed when making cross-origin requests
            if (_allowedMethods.Count > 0)
            {
                webContext.WebResponse.Headers["Access-Control-Allow-Methods"] = string.Join(", ", _allowedMethods);
                logs.Add($"Allowed methods are set. methods={string.Join(", ", _allowedMethods)}.");
            }
            //browser blocks requests containing headers outside of this list + its small set of default headers
            if (_allowedHeaders.Count > 0)
            {
                webContext.WebResponse.Headers["Access-Control-Allow-Headers"] = string.Join(", ", _allowedHeaders);
                logs.Add($"Allowed headers are set. headers={string.Join(", ", _allowedHeaders)}.");
            }
            //how long the browser should cache the OPTIONS response.
            if (_preflightMaxAgeSeconds > 0)
            {
                webContext.WebResponse.Headers["Access-Control-Max-Age"] = _preflightMaxAgeSeconds.ToString(CultureInfo.InvariantCulture);
                logs.Add($"Preflight max age is set. maxAgeSeconds={_preflightMaxAgeSeconds}.");
            }

            webContext.WebResponse.StatusCode = 204;
            webContext.WebResponse.ResponsePhrase = "No content";

            logs.Add("Preflight request matched. status=204.");
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.ShortCircuit, downstreamStart, logs);
            return;
        }

        RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, logs);
        await _nextFunction(webContext, cancellationToken);

        var upstreamStart = DateTimeOffset.UtcNow;
        var upstreamLogs = new List<string>();

        //add "Origin" to Vary header. Cache now needs to check request Origin before serving cached content
        webContext.WebResponse.ApplyVaryHeader("Origin");
        upstreamLogs.Add("Append Origin to Vary header for external caching.");

        //allow sending to origin
        webContext.WebResponse.Headers["Access-Control-Allow-Origin"] = origin;
        upstreamLogs.Add($"Allowed origin is set. origin={origin}.");

        //browser-stored credentials are sent with the request (session cookies, http auth,...)
        if (_allowCredentials)
        {
            webContext.WebResponse.Headers["Access-Control-Allow-Credentials"] = "true";
            upstreamLogs.Add("Allow CORS credentials.");
        }

        //allow JS to read these headers
        if (_exposedHeaders.Count > 0)
        {
            webContext.WebResponse.Headers["Access-Control-Expose-Headers"] = string.Join(", ", _exposedHeaders);
            upstreamLogs.Add($"Expose CORS headers. headers={string.Join(", ", _exposedHeaders)}.");
        }

        RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, upstreamLogs);
    }
}
