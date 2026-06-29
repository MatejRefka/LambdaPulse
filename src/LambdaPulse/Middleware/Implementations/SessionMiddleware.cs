using LambdaPulse.Engine.Features.Logging;
using LambdaPulse.Engine.Features.State.Sessions;
using LambdaPulse.Engine.Configuration;
using LambdaPulse.Engine.Http.Abstractions;

namespace LambdaPulse.Engine.Middleware.Implementations;

/// <summary>
/// Reads sessionId from request cookies and loads session data from the session store.
/// Upstream, the session data is saved back to the session store.
/// </summary>
internal sealed class SessionMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "Session";

    private readonly ISessionStore _sessionStore;
    private readonly bool _cookieSecure;

    public SessionMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, ISessionStore sessionStore, IConfigProvider configProvider) : base(nextFunction)
    {
        _sessionStore = sessionStore;
        _cookieSecure = configProvider.ServerConfig.MiddlewareConfig.SessionMiddleware.CookieSecure;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var downstreamStart = DateTimeOffset.UtcNow;
        var downstreamLogs = new List<string>();

        webContext.WebRequest.Cookies.TryGetValue(SessionConstants.SessionCookieName, out var sessionId);
        downstreamLogs.Add(string.IsNullOrWhiteSpace(sessionId) ? "Session cookie is missing. Create session." : $"Session cookie is present. Load session. cookie={SessionConstants.SessionCookieName}.");

        var requestSession = string.IsNullOrWhiteSpace(sessionId) ? CreateSession() : await _sessionStore.GetSession(sessionId, cancellationToken);
        downstreamLogs.Add(string.IsNullOrWhiteSpace(sessionId) ? "No session cookie was provided. Use new session." : (requestSession != null ? "Session id matched the store. Use stored session." : "Session id not found in store. Use new session."));

        //sessionId from client not found in store
        requestSession ??= CreateSession();

        webContext.Session = requestSession;

        RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, downstreamLogs);

        await _nextFunction(webContext, cancellationToken);

        var upstreamStart = DateTimeOffset.UtcNow;
        var upstreamLogs = new List<string>();

        var sessionIsNew = webContext.Session.IsNew;

        await _sessionStore.SaveSession(webContext.Session, cancellationToken);
        upstreamLogs.Add("Store session in store.");

        if (sessionIsNew)
        {
            var secureAttribute = _cookieSecure ? "; Secure" : string.Empty;
            webContext.WebResponse.Cookies.Add($"{SessionConstants.SessionCookieName}={webContext.Session.Id}; Path=/; HttpOnly{secureAttribute}");
            upstreamLogs.Add($"Issue session cookie. cookie={SessionConstants.SessionCookieName}.");
        }

        RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, upstreamLogs);
    }

    private static Session CreateSession()
    {
        return new Session(Guid.CreateVersion7(DateTimeOffset.UtcNow).ToString("N"))
        {
            IsNew = true
        };
    }
}
