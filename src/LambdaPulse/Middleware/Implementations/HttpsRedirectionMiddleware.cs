namespace LambdaPulse.Middleware
{
    public class HttpsRedirection : MiddlewareBase
    {
        public HttpsRedirection(Action<string> nextFunction) : base(nextFunction)
        {
            _nextFunction = nextFunction;
        }

        public override void Invoke(string data)
        {
            Console.WriteLine($"[HttpsRedirection] logic performed on {data}");
            _nextFunction(data);
            Console.WriteLine($"[HttpsRedirection] logic performed on {data}");
        }
    }
}
