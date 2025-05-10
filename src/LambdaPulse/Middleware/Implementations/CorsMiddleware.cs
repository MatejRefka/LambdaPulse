namespace LambdaPulse.Middleware.Implementations
{
    public class Authorization : MiddlewareBase
    {
        public Authorization(Func<WebContext, Task> nextFunction) : base(nextFunction)
        {
            _nextFunction = nextFunction;
        }

        public override async Task Invoke(WebContext webContext)
        {
            Console.WriteLine($"[Authorization] logic performed on {webContext.WebRequest.Payload}");
            await _nextFunction(webContext);
            Console.WriteLine($"[Authorization] logic performed on {webContext.WebResponse.Payload}");
        }
    }
}
