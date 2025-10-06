using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Middleware.Implementations;

public sealed class CSRF : MiddlewareBase
{
    public CSRF(Func<WebContext, Task> nextFunction) : base(nextFunction)
    {
        _nextFunction = nextFunction;
    }

    public override async Task Invoke(WebContext webContext)
    {
        Console.WriteLine($"[CSRF] logic performed on WebRequest");
        await _nextFunction(webContext);
        Console.WriteLine($"[CSRF] logic performed on WebResponse");
    }
}
