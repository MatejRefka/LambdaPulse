using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Middleware.Implementations
{
    public sealed class CORS : MiddlewareBase
    {
        public CORS(Func<WebContext, Task> nextFunction) : base(nextFunction)
        {
            _nextFunction = nextFunction;
        }

        public override async Task Invoke(WebContext webContext)
        {
            Console.WriteLine($"[CORS] logic performed on {webContext.WebRequest.Payload}");
            await _nextFunction(webContext);
            Console.WriteLine($"[CORS] logic performed on {webContext.WebResponse.Payload}");
        }
    }
}
