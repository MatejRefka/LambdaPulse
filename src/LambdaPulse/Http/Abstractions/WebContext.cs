using LambdaPulse.Engine.Features.Authentication;
using LambdaPulse.Engine.Features.Logging;
using LambdaPulse.Engine.Features.Routing;
using LambdaPulse.Engine.Features.State.Sessions;

namespace LambdaPulse.Engine.Http.Abstractions;

public sealed class WebContext
{
    public string? RemoteIpAddress { get; init; }
    public required WebRequest WebRequest { get; init; }
    public required WebResponse WebResponse { get; init; }
    public string? PreSessionToken { get; set; }
    public string? AnonymousSessionToken { get; set; }
    public Session? Session { get; set; }
    public IUser User { get; set; } = GuestUser.Instance;
    public Endpoint? Endpoint { get; set; }
    public string? StaticFileRelativePath { get; set; }
    public string? NegotiatedMimeType { get; set; }
    public bool ConnectionCloseRequested { get; set; }
    public required Trace Trace { get; set; }
}
