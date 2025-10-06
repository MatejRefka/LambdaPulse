using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Middleware.Implementations;

public sealed class HttpsRedirection : MiddlewareBase
{
    public HttpsRedirection(Func<WebContext, Task> nextFunction) : base(nextFunction)
    {
        _nextFunction = nextFunction;
    }

    public override async Task Invoke(WebContext webContext)
    {
        Console.WriteLine($"[HttpsRedirection] logic performed on WebRequest");
        await _nextFunction(webContext);
        Console.WriteLine($"[HttpsRedirection] logic performed on WebResponse");
    }
}
