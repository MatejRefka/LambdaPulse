using LambdaPulse.Server.Http.Abstractions;

namespace LambdaPulse.Server.Middleware.Implementations;

public sealed class CorsMiddleware : MiddlewareBase
{
    public CorsMiddleware(Func<WebContext, CancellationToken, Task> nextFunction) : base(nextFunction)
    {
        _nextFunction = nextFunction;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        await _nextFunction(webContext, cancellationToken);
    }
}
