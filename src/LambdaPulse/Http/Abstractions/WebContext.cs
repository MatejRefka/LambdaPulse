using LambdaPulse.Services.Http.State;
namespace LambdaPulse.Services.Http.Models;

public sealed class WebContext
{
    public required WebRequest WebRequest { get; init; }
    public required WebResponse WebResponse { get; init; }
    public Session? Session { get; set; }
    public bool ConnectionCloseRequested { get; set; }
}
