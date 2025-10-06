using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Middleware.Implementations;

public sealed class RequestLimits : MiddlewareBase
{
    public RequestLimits(Func<WebContext, Task> nextFunction) : base(nextFunction)
    {
        _nextFunction = nextFunction;
    }

    public override async Task Invoke(WebContext webContext)
    {
        Console.WriteLine($"[RequestLimits] logic performed on WebRequest");
        await _nextFunction(webContext);
        Console.WriteLine($"[RequestLimits] logic performed on WebResponse");
    }
}
