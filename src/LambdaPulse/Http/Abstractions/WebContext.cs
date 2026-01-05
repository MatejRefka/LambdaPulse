using LambdaPulse.Server.Services.Http.Authentication;
using LambdaPulse.Server.Services.Http.Routing;
using LambdaPulse.Server.Services.Http.State;

namespace LambdaPulse.Server.Services.Http.Models;

public sealed class WebContext
{
    public required WebRequest WebRequest { get; init; }
    public required WebResponse WebResponse { get; init; }
    public Session? Session { get; set; }
    public IUser User { get; set; } = GuestUser.Instance;
    public Endpoint? Endpoint { get; set; }
    public string? StaticFileRelativePath { get; set; }
    public bool ConnectionCloseRequested { get; set; }
}
