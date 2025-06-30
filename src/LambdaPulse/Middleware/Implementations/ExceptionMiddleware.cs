using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Middleware.Implementations
{
    public class ExceptionHandler : MiddlewareBase
    {
        public ExceptionHandler(Func<WebContext, Task> nextFunction) : base(nextFunction)
        {
            _nextFunction = nextFunction;
        }

        public override async Task Invoke(WebContext webContext)
        {
            Console.WriteLine($"[ExceptionHandler] logic performed on {webContext.WebRequest.Payload}");
            await _nextFunction(webContext);
            Console.WriteLine($"[ExceptionHandler] logic performed on {webContext.WebResponse.Payload}");
        }
    }
}
