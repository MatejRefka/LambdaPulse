namespace LambdaPulse.Middleware
{
    public class HttpsRedirection : MiddlewareBase
    {
        public HttpsRedirection(Func<WebContext, Task> nextFunction) : base(nextFunction)
        {
            _nextFunction = nextFunction;
        }

        public override async Task Invoke(WebContext webContext)
        {
            Console.WriteLine($"[HttpsRedirection] logic performed on {webContext.WebRequest.Payload}");
            await _nextFunction(webContext);
            Console.WriteLine($"[HttpsRedirection] logic performed on {webContext.WebResponse.Payload}");
        }
    }
}
