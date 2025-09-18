using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Middleware.Implementations
{
    public sealed class Connection : MiddlewareBase
    {
        public Connection(Func<WebContext, Task> nextFunction) : base(nextFunction)
        {
            _nextFunction = nextFunction;
        }

        public override async Task Invoke(WebContext webContext)
        {
            Console.WriteLine($"[Connection] logic performed on WebRequest");
            await _nextFunction(webContext);
            Console.WriteLine($"[Connection] logic performed on WebResponse");
        }
    }
}
