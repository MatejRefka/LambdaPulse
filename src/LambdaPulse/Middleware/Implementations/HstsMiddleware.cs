namespace LambdaPulse.Middleware.Implementations
{
    public class HSTS : MiddlewareBase
    {
        public HSTS(Func<WebContext, Task> nextFunction) : base(nextFunction)
        {
            _nextFunction = nextFunction;
        }

        public override async Task Invoke(WebContext webContext)
        {
            Console.WriteLine($"[HSTS] logic performed on {webContext.WebRequest.Payload}");
            await _nextFunction(webContext);
            Console.WriteLine($"[HSTS] logic performed on {webContext.WebResponse.Payload}");
        }
    }
}
