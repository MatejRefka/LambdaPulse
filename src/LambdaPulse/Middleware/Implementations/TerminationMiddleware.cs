using LambdaPulse.Server.Http.Abstractions;
using LambdaPulse.Server.Shared.Extensions;

namespace LambdaPulse.Server.Middleware.Implementations;

public sealed class TerminationMiddleware : MiddlewareBase
{
    public TerminationMiddleware(Func<WebContext, CancellationToken, Task> nextFunction) : base(nextFunction)
    {
        _nextFunction = nextFunction;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        if (!webContext.WebResponse.HasStarted)
        {
            webContext.WebResponse.StatusCode = 404;
            webContext.WebResponse.ResponsePhrase = "Not Found";
            await webContext.WebResponse.WriteStringToBody("Not Found", cancellationToken);
        }
    }
}