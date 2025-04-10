namespace LambdaPulse.Middleware
{
    public class Authorization : MiddlewareBase
    {
        public Authorization(Action<string> nextFunction) : base(nextFunction)
        {
            _nextFunction = nextFunction;
        }

        public override void Invoke(string data)
        {
            Console.WriteLine($"[Authorization] logic performed on {data}");
            _nextFunction(data);
            Console.WriteLine($"[Authorization] logic performed on {data}");
        }
    }
}
