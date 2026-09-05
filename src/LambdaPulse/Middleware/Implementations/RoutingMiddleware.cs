using LambdaPulse.Features.Logging;
using LambdaPulse.Features.Routing;
using LambdaPulse.Http.Abstractions;

namespace LambdaPulse.Middleware.Implementations;

/// <summary>
/// Determines whether the request matches any registered endpoints based on the HTTP method and request path.
/// </summary>
internal sealed class RoutingMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "Routing";

    private readonly IEndpointRegistry _endpointRegistry;

    public RoutingMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, IEndpointRegistry endpointRegistry) : base(nextFunction)
    {
        _endpointRegistry = endpointRegistry;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var downstreamStart = DateTimeOffset.UtcNow;

        var endpoint = _endpointRegistry.GetEndpoint(webContext.WebRequest.Method, webContext.WebRequest.Path);

        webContext.Endpoint = endpoint;

        RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { endpoint == null ? $"No route matched {webContext.WebRequest.Method} {webContext.WebRequest.Path}." : $"Endpoint found for {webContext.WebRequest.Method} {webContext.WebRequest.Path}." });
        await _nextFunction(webContext, cancellationToken);
        RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
    }
}
