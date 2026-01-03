using LambdaPulse.Server.Services.Http.Models;

namespace LambdaPulse.Server.Middleware.Implementations;

public sealed class ResponseCompressionMiddleware : MiddlewareBase
{
    public ResponseCompressionMiddleware(Func<WebContext, CancellationToken, Task> nextFunction) : base(nextFunction)
    {
        _nextFunction = nextFunction;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        await _nextFunction(webContext, cancellationToken);
    }
}
