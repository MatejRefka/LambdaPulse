namespace LambdaPulse.Middleware
{
    public class Endpoint : MiddlewareBase
    {
        public Endpoint(Action<string> nextFunction) : base(nextFunction)
        {
            _nextFunction = nextFunction;
        }

        public override void Invoke(string data)
        {
            Console.WriteLine($"[Endpoint] logic performed on {data}");
        }
    }
}
