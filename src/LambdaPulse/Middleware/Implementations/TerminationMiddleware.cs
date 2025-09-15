using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Middleware.Implementations
{
    public sealed class Endpoint : MiddlewareBase
    {
        public Endpoint(Func<WebContext, Task> nextFunction) : base(nextFunction)
        {
            _nextFunction = nextFunction;
        }

        public override async Task Invoke(WebContext webContext)
        {
            Console.WriteLine($"[Endpoint] logic performed on WebRequest");
            await Task.CompletedTask;
        }
    }
}
