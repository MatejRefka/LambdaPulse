using LambdaPulse.Features.Authentication;
using LambdaPulse.Features.Logging;
using LambdaPulse.Features.Routing;
using LambdaPulse.Features.State.Sessions;

namespace LambdaPulse.Http.Abstractions;

/// <summary>
/// Context for a HTTP request, response and the associated state passing through the middleware pipeline.
/// </summary>
public sealed class WebContext
{
    /// <summary>
    /// IP address of the client making the request.
    /// </summary>
    public string? RemoteIpAddress { get; init; }

    /// <summary>
    /// HTTP request representation.
    /// </summary>
    public required WebRequest WebRequest { get; init; }

    /// <summary>
    /// HTTP response representation.
    /// </summary>
    public required WebResponse WebResponse { get; init; }

    /// <summary>
    /// Pre-session token extracted from the request headers or cookies, used for anonymous session management.
    /// </summary>
    public string? PreSessionToken { get; set; }

    /// <summary>
    /// Anonymous session token extracted from the request headers or cookies, used for anonymous session management.
    /// </summary>
    public string? AnonymousSessionToken { get; set; }

    /// <summary>
    /// User session storing key-value pairs.
    /// </summary>
    public Session? Session { get; set; }

    /// <summary>
    /// Authenticated user associated with the request. Defaults to a guest user if not authenticated.
    /// </summary>
    public IUser User { get; set; } = GuestUser.Instance;

    /// <summary>
    /// Endpoint matching the incoming request.
    /// </summary>
    public Endpoint? Endpoint { get; set; }

    /// <summary>
    /// Relative path to the static file being served.
    /// </summary>
    public string? StaticFileRelativePath { get; set; }

    /// <summary>
    /// Negotiated MIME type for the response, based on the request's Accept header and server capabilities.
    /// </summary>
    public string? NegotiatedMimeType { get; set; }

    /// <summary>
    /// Indicates whether the connection should be closed after the response is sent.
    /// </summary>
    public bool ConnectionCloseRequested { get; set; }

    /// <summary>
    /// Indicates whether the session should be invalidated after the response is sent.
    /// </summary>
    public bool SessionInvalidationRequested { get; set; }

    /// <summary>
    /// Trace of the HTTP request and response.
    /// </summary>
    public required Trace Trace { get; set; }
}
