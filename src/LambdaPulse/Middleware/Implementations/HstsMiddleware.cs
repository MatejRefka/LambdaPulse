using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Middleware.Implementations;

public sealed class HSTS : MiddlewareBase
{
    public HSTS(Func<WebContext, Task> nextFunction) : base(nextFunction)
    {
        _nextFunction = nextFunction;
    }

    public override async Task Invoke(WebContext webContext)
    {
        Console.WriteLine($"[HSTS] logic performed on WebRequest");
        await _nextFunction(webContext);
        Console.WriteLine($"[HSTS] logic performed on WebResponse");
    }
}
