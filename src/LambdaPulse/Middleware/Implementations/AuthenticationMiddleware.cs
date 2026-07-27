using LambdaPulse.Features.Authentication;
using LambdaPulse.Features.Logging;
using LambdaPulse.Http.Abstractions;

namespace LambdaPulse.Middleware.Implementations;

/// <summary>
/// Sets the User by authenticating the incoming request using the configured authentication scheme.
/// Session authentication is the default scheme.
/// </summary>
internal sealed class AuthenticationMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "Authentication";

    private readonly IAuthenticationScheme _authenticationScheme;

    public AuthenticationMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, IAuthenticationScheme authenticationScheme) : base(nextFunction)
    {
        _authenticationScheme = authenticationScheme;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var downstreamStart = DateTimeOffset.UtcNow;

        webContext.User = await _authenticationScheme.Authenticate(webContext, cancellationToken);

        if (webContext.User.IsAuthenticated)
        {
            webContext.Trace.UserId = webContext.User.Id;
        }

        RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, webContext.User.IsAuthenticated ? new List<string> { "User authenticated." } : new List<string> { "Authentication scheme did not return an authenticated user. Authenticate as guest." });
        await _nextFunction(webContext, cancellationToken);
        RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
    }
}
