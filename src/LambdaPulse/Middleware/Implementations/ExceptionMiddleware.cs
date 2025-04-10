namespace LambdaPulse.Middleware
{
    public class ExceptionHandler : MiddlewareBase
    {
        public ExceptionHandler(Action<string> nextFunction) : base(nextFunction)
        {
            _nextFunction = nextFunction;
        }

        public override void Invoke(string data)
        {
            Console.WriteLine($"[ExceptionHandler] logic performed on {data}");
            _nextFunction(data);
            Console.WriteLine($"[ExceptionHandler] logic performed on {data}");
        }
    }
}
