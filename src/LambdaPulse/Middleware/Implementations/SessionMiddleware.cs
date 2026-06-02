using LambdaPulse.Engine.Features.Logging;
using LambdaPulse.Engine.Features.State.Sessions;
using LambdaPulse.Engine.Http.Abstractions;

namespace LambdaPulse.Engine.Middleware.Implementations;

/// <summary>
/// Reads sessionId from request cookies and loads session data from the session store.
/// Upstream, the session data is saved back to the session store.
/// </summary>
internal sealed class SessionMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "Session";

    private const string SessionCookieName = "LambdaPulse.Session";
    private readonly ISessionStore _sessionStore;

    public SessionMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, ISessionStore sessionStore) : base(nextFunction)
    {
        _sessionStore = sessionStore;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var downstreamStart = DateTimeOffset.UtcNow;
        var downstreamLogs = new List<string>();

        webContext.WebRequest.Cookies.TryGetValue(SessionCookieName, out var sessionId);
        downstreamLogs.Add(string.IsNullOrWhiteSpace(sessionId) ? "Session cookie not present." : $"Session cookie '{SessionCookieName}' present.");

        var requestSession = string.IsNullOrWhiteSpace(sessionId) ? CreateSession() : await _sessionStore.GetSession(sessionId, cancellationToken);
        downstreamLogs.Add(string.IsNullOrWhiteSpace(sessionId) ? "New session created." : (requestSession != null ? $"Session found in store." : $"Session not found in store. New session created."));

        //sessionId from client not found in store
        requestSession ??= CreateSession();

        webContext.Session = requestSession;

        RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, downstreamLogs);

        await _nextFunction(webContext, cancellationToken);

        var upstreamStart = DateTimeOffset.UtcNow;
        var upstreamLogs = new List<string>();

        var sessionIsNew = webContext.Session.IsNew;

        await _sessionStore.SaveSession(webContext.Session, cancellationToken);
        upstreamLogs.Add("Session saved to store.");

        if (sessionIsNew)
        {
            webContext.WebResponse.Cookies.Add($"{SessionCookieName}={webContext.Session.Id}; Path=/; HttpOnly; Secure");
            upstreamLogs.Add($"Set '{SessionCookieName}=xyz; Path=/; HttpOnly; Secure' cookie.");
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
