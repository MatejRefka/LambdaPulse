using LambdaPulse.Middleware;
using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Middleware.Implementations
{
    public class Authentication : MiddlewareBase
    {
        public Authentication(Func<WebContext, Task> nextFunction) : base(nextFunction)
        {
            _nextFunction = nextFunction;
        }

        public override async Task Invoke(WebContext webContext)
        {
            Console.WriteLine($"[Authentication] logic performed on {webContext.WebRequest.Payload}");
            await _nextFunction(webContext);
            Console.WriteLine($"[Authentication] logic performed on {webContext.WebResponse.Payload}");
        }
    }
}
