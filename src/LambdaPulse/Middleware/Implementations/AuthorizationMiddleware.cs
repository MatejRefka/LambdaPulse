using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Middleware.Implementations;

public sealed class ContentNegotiation : MiddlewareBase
{
    public ContentNegotiation(Func<WebContext, CancellationToken, Task> nextFunction) : base(nextFunction)
    {
        _nextFunction = nextFunction;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[ContentNegotiation] logic performed on WebRequest");
        await _nextFunction(webContext, cancellationToken);
        Console.WriteLine($"[ContentNegotiation] logic performed on WebResponse");
    }
}
