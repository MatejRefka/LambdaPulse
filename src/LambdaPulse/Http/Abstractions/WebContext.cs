using LambdaPulse.Server.Features.Authentication;
using LambdaPulse.Server.Features.Logging;
using LambdaPulse.Server.Features.Routing;
using LambdaPulse.Server.Features.State;

namespace LambdaPulse.Server.Http.Abstractions;

public sealed class WebContext
{
    public required WebRequest WebRequest { get; init; }
    public required WebResponse WebResponse { get; init; }
    public Session? Session { get; set; }
    public IUser User { get; set; } = GuestUser.Instance;
    public Endpoint? Endpoint { get; set; }
    public string? StaticFileRelativePath { get; set; }
    public string? NegotiatedMimeType { get; set; }
    public bool ConnectionCloseRequested { get; set; }
    public required Trace Trace { get; set; }
}
