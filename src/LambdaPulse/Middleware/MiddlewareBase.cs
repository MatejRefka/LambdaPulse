using LambdaPulse.Server.Http.Abstractions;

namespace LambdaPulse.Server.Middleware;

internal abstract class MiddlewareBase
{
    //delegate pointing to the next function in the pipeline chain
    protected Func<WebContext, CancellationToken, Task> _nextFunction;

    public MiddlewareBase(Func<WebContext, CancellationToken, Task> nextFunction)
    {
        _nextFunction = nextFunction;
    }

    //custom logic of the implementing middleware
    public abstract Task Invoke(WebContext webContext, CancellationToken cancellationToken = default);
}
