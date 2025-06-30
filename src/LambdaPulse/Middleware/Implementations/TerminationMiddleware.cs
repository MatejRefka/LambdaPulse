using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Middleware.Implementations
{
    public class Endpoint : MiddlewareBase
    {
        public Endpoint(Func<WebContext, Task> nextFunction) : base(nextFunction)
        {
            _nextFunction = nextFunction;
        }

        public override async Task Invoke(WebContext webContext)
        {
            Console.WriteLine($"[Endpoint] logic performed on {webContext.WebRequest.Payload}");
            await Task.CompletedTask;
        }
    }
}
