using LambdaPulse.Server.Services.Http.Models;
using LambdaPulse.Server.Services.Http.Routing;

namespace LambdaPulse.Server.Middleware.Implementations;

public sealed class RoutingMiddleware : MiddlewareBase
{
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
