using LambdaPulse.Configuration;
using LambdaPulse.Features.Logging;
using LambdaPulse.Http.Abstractions;

namespace LambdaPulse.Middleware.Implementations;

/// <summary>
/// Serves a fallback web page when no endpoint is matched.
/// Applies to GET requests accepting HTML. Does not apply to file requests and API routes.
/// </summary>
internal sealed class SpaFallbackMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "SPA Fallback";

    private readonly string? _indexPageRelativePath;

    public SpaFallbackMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, Config config) : base(nextFunction)
    {
        _indexPageRelativePath = config.ServerConfig.MiddlewareConfig.SpaFallbackConfig.IndexPageRelativePath;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var downstreamStart = DateTimeOffset.UtcNow;

        if (_indexPageRelativePath == null)
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "SPA fallback index is not configured. Skip SPA fallback." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //skip non-GET requests
        if (!string.Equals(webContext.WebRequest.Method, "GET", StringComparison.OrdinalIgnoreCase))
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { $"{webContext.WebRequest.Method} request. Skip SPA fallback." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //skip if endpoint already matched
        if (webContext.Endpoint != null)
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "Endpoint already matched. Skip SPA fallback." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //skip static file request
        if (Path.HasExtension(webContext.WebRequest.Path))
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "Request targets a file path. Skip SPA fallback." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //skip if request doesn't accept text/html
        if (!webContext.WebRequest.Headers.TryGetValue("Accept", out var value) || !value.Contains("text/html", StringComparison.OrdinalIgnoreCase))
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "Request does not accept text/html. Skip SPA fallback." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //skip if path starts with /api
        if (webContext.WebRequest.Path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "Request targets an API route. Skip SPA fallback." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //indicate to serve the fallback file
        webContext.StaticFileRelativePath = _indexPageRelativePath;
        RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.ShortCircuit, downstreamStart, new List<string> { $"Map request to fallback. {webContext.WebRequest.Path} -> {_indexPageRelativePath}." });

        return;
    }
}
