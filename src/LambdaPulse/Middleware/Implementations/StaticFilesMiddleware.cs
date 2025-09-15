using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Middleware.Implementations
{
    public sealed class StaticFiles : MiddlewareBase
    {
        public StaticFiles(Func<WebContext, Task> nextFunction) : base(nextFunction)
        {
            _nextFunction = nextFunction;
        }

        public override async Task Invoke(WebContext webContext)
        {
            Console.WriteLine($"[StaticFiles] logic performed on WebRequest");
            await _nextFunction(webContext);
            Console.WriteLine($"[StaticFiles] logic performed on WebResponse");
        }
    }
}
