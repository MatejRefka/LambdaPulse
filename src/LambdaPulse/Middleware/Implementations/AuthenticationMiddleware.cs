namespace LambdaPulse.Middleware
{
    public class Authentication : MiddlewareBase
    {
        public Authentication(Action<string> nextFunction) : base(nextFunction)
        {
            _nextFunction = nextFunction;
        }

        public override void Invoke(string data)
        {
            Console.WriteLine($"[Authentication] logic performed on {data}");
            _nextFunction(data);
            Console.WriteLine($"[Authentication] logic performed on {data}");
        }
    }
}
