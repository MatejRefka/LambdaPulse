using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Middleware.Implementations
{
    public sealed class Security : MiddlewareBase
    {
        public Security(Func<WebContext, Task> nextFunction) : base(nextFunction)
        {
            _nextFunction = nextFunction;
        }

        public override async Task Invoke(WebContext webContext)
        {
            Console.WriteLine($"[Security] logic performed on WebRequest");
            await _nextFunction(webContext);
            Console.WriteLine($"[Security] logic performed on WebResponse");
        }
    }
}
