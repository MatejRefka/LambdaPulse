namespace LambdaPulse
{
    public class WebContext
    {
        public required WebRequest WebRequest { get; init; }

        public required WebResponse WebResponse { get; init; }
    }
}
