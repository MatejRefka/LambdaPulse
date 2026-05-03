using LambdaPulse.Engine.Features.Routing;
using LambdaPulse.Engine.Http.Abstractions;

namespace LambdaPulse.Engine.Middleware.Implementations;

internal sealed class RoutingMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "Routing";

    private readonly EndpointRegistry _endpointRegistry;

    public RoutingMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, EndpointRegistry endpointRegistry) : base(nextFunction)
    {
        _nextFunction = nextFunction;
        _endpointRegistry = endpointRegistry;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var endpoint = _endpointRegistry.GetEndpoint(webContext.WebRequest.Method, webContext.WebRequest.Path);

        webContext.Endpoint = endpoint;

        await _nextFunction(webContext, cancellationToken);
    }
}
