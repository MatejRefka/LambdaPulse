using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Middleware.Implementations;

public sealed class Connection : MiddlewareBase
{
    public Connection(Func<WebContext, CancellationToken, Task> nextFunction) : base(nextFunction)
    {
        _nextFunction = nextFunction;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Connection] logic performed on WebRequest");
        await _nextFunction(webContext, cancellationToken);
        Console.WriteLine($"[Connection] logic performed on WebResponse");
    }
}
