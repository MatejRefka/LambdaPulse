using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Middleware.Implementations;
/// <summary>
/// Executes user code that's mapped to the requested endpoint.
/// </summary>
public class InvokeMiddleware : MiddlewareBase
{
    public InvokeMiddleware(Func<WebContext, CancellationToken, Task> nextFunction) : base(nextFunction)
    {
        _nextFunction = nextFunction;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        if (webContext.Endpoint != null)
        {
            //user application code is invoked here
            await webContext.Endpoint.ApplicationFunction(webContext, cancellationToken);
            return;
        }

        await _nextFunction(webContext, cancellationToken);
    }
}
