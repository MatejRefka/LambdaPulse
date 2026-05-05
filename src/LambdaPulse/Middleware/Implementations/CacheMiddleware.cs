using LambdaPulse.Engine.Http.Abstractions;

namespace LambdaPulse.Engine.Middleware.Implementations;

internal sealed class CacheMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "Cache";

    public CacheMiddleware(Func<WebContext, CancellationToken, Task> nextFunction) : base(nextFunction)
    {
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        await _nextFunction(webContext, cancellationToken);
    }
}
