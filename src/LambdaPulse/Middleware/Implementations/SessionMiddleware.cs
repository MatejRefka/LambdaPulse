using LambdaPulse.Engine.Configuration;
using LambdaPulse.Engine.Features.State;
using LambdaPulse.Engine.Http.Abstractions;

namespace LambdaPulse.Engine.Middleware.Implementations;

internal sealed class SessionMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "Session";

    private const string SessionCookieName = "LambdaPulse.Session";
    private readonly ISessionStore _sessionStore;

    public SessionMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, IConfigProvider configProvider, ISessionStore sessionStore) : base(nextFunction)
    {
        _sessionStore = sessionStore;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        webContext.WebRequest.Cookies.TryGetValue(SessionCookieName, out var sessionId);

        var requestSession = string.IsNullOrWhiteSpace(sessionId) ? CreateSession() : _sessionStore.GetSession(sessionId);

        //sessionId from client not found in store
        requestSession ??= CreateSession();

        webContext.Session = requestSession;

        await _nextFunction(webContext, cancellationToken);

        _sessionStore.SaveSession(webContext.Session);

        if (webContext.Session.IsNew)
        {
            webContext.WebResponse.Cookies.Add($"{SessionCookieName}={webContext.Session.Id}; Path=/; HttpOnly; Secure");
        }
    }

    private static Session CreateSession()
    {
        return new Session(Guid.CreateVersion7(DateTimeOffset.UtcNow).ToString("N"))
        {
            IsNew = true
        };
    }
}
