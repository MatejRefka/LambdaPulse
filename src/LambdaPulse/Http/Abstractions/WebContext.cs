namespace LambdaPulse.Services.Http.Models
{
    public sealed class WebContext
    {
        public required WebRequest WebRequest { get; init; }

        public required WebResponse WebResponse { get; init; }
    }
}
