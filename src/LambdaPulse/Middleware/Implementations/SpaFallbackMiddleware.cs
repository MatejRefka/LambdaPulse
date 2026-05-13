using LambdaPulse.Engine.Configuration;
using LambdaPulse.Engine.Features.Logging;
using LambdaPulse.Engine.Http.Abstractions;

namespace LambdaPulse.Engine.Middleware.Implementations;

/// <summary>
/// Serves a fallback web page when no endpoint is matched.
/// Applies to GET requests accepting HTML. Does not apply to file requests and API routes.
/// </summary>
internal sealed class SpaFallbackMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "SPA Fallback";

    private readonly string _indexPageRelativePath;

    public SpaFallbackMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, IConfigProvider configProvider) : base(nextFunction)
    {
        _indexPageRelativePath = configProvider.ServerConfig.MiddlewareConfig.SpaFallbackMiddleware.IndexPageRelativePath;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var downstreamStart = DateTimeOffset.UtcNow;

        //skip non-GET requests
        if (!string.Equals(webContext.WebRequest.Method, "GET", StringComparison.OrdinalIgnoreCase))
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { $"{webContext.WebRequest.Method} request. SPA fallback skipped." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //skip if endpoint already matched
        if (webContext.Endpoint != null)
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { $"Endpoint already matched. SPA fallback skipped." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //skip static file request
        if (Path.HasExtension(webContext.WebRequest.Path))
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { $"File request. SPA fallback skipped." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //skip if request doesn't accept text/html
        if (!webContext.WebRequest.Headers.TryGetValue("Accept", out var value) || !value.Contains("text/html", StringComparison.OrdinalIgnoreCase))
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { $"Request does not accept text/html. SPA fallback skipped." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //skip if path starts with /api
        if (webContext.WebRequest.Path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { $"API route. SPA fallback skipped." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //indicate to serve the fallback file
        webContext.StaticFileRelativePath = _indexPageRelativePath;
        RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.ShortCircuit, downstreamStart, new List<string> { $"Mapped SPA route '{webContext.WebRequest.Path}' to '{_indexPageRelativePath}'." });

        return;
    }
}
