namespace LambdaPulse.Middleware
{
    public class HSTS : MiddlewareBase
    {
        public HSTS(Action<string> nextFunction) : base(nextFunction)
        {
            _nextFunction = nextFunction;
        }

        public override void Invoke(string data)
        {
            Console.WriteLine($"[HSTS] logic performed on {data}");
            _nextFunction(data);
            Console.WriteLine($"[HSTS] logic performed on {data}");
        }
    }
}
