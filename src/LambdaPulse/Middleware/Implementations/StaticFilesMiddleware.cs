namespace LambdaPulse.Middleware
{
    public class StaticFiles : MiddlewareBase
    {
        public StaticFiles(Action<string> nextFunction) : base(nextFunction)
        {
            _nextFunction = nextFunction;
        }

        public override void Invoke(string data)
        {
            Console.WriteLine($"[StaticFiles] logic performed on {data}");
            _nextFunction(data);
            Console.WriteLine($"[StaticFiles] logic performed on {data}");
        }
    }
}
