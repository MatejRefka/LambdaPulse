namespace LambdaPulse.Middleware
{
    public class StaticFiles : MiddlewareBase
    {
        public StaticFiles(Func<WebContext, Task> nextFunction) : base(nextFunction)
        {
            _nextFunction = nextFunction;
        }

        public override async Task Invoke(WebContext webContext)
        {
            Console.WriteLine($"[StaticFiles] logic performed on {webContext.WebRequest.Payload}");
            await _nextFunction(webContext);
            Console.WriteLine($"[StaticFiles] logic performed on {webContext.WebResponse.Payload}");
        }
    }
}
