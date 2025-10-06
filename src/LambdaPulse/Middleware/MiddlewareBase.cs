using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Middleware;

public abstract class MiddlewareBase
{
    //delegate pointing to the next function in the pipeline chain
    protected Func<WebContext, Task> _nextFunction;

    public MiddlewareBase(Func<WebContext, Task> nextFunction)
    {
        _nextFunction = nextFunction;
    }

    //custom logic of the implementing middleware
    public abstract Task Invoke(WebContext webContext);
}
