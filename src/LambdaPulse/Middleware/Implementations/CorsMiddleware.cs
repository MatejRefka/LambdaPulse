using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Middleware.Implementations;

public sealed class Authorization : MiddlewareBase
{
    public Authorization(Func<WebContext, Task> nextFunction) : base(nextFunction)
    {
        _nextFunction = nextFunction;
    }

    public override async Task Invoke(WebContext webContext)
    {
        Console.WriteLine($"[Authorization] logic performed on WebRequest");
        await _nextFunction(webContext);
        Console.WriteLine($"[Authorization] logic performed on WebResponse");
    }
}
