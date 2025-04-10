namespace LambdaPulse.Middleware
{
    public class CORS : MiddlewareBase
    {
        public CORS(Action<string> nextFunction) : base(nextFunction)
        {
            _nextFunction = nextFunction;
        }

        public override void Invoke(string data)
        {
            Console.WriteLine($"[CORS] logic performed on {data}");
            _nextFunction(data);
            Console.WriteLine($"[CORS] logic performed on {data}");
        }
    }
}
