using LambdaPulse.Configuration;
using LambdaPulse.Services.Http.Models;
using LambdaPulse.Services.Http.State;

namespace LambdaPulse.Middleware.Implementations;

public sealed class SessionMiddleware : MiddlewareBase
{
    private const string SessionCookieName = "LambdaPulse.Session";
    private readonly ISessionStore _sessionStore;

    public SessionMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, IConfigProvider configProvider, ISessionStore sessionStore) : base(nextFunction)
    {
        _nextFunction = nextFunction;
        _sessionStore = sessionStore;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        webContext.WebRequest.Cookies.TryGetValue(SessionCookieName, out var sessionId);

        var requestSession = string.IsNullOrEmpty(sessionId) ? CreateSession() : _sessionStore.GetSession(sessionId);

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
