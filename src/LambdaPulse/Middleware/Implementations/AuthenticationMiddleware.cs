using LambdaPulse.Server.Features.Authentication;
using LambdaPulse.Server.Http.Abstractions;

namespace LambdaPulse.Server.Middleware.Implementations;

public sealed class AuthenticationMiddleware : MiddlewareBase
{
    private const string UserIdSessionKey = "auth.user_id";

    public AuthenticationMiddleware(Func<WebContext, CancellationToken, Task> nextFunction) : base(nextFunction)
    {
        _nextFunction = nextFunction;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var session = webContext.Session;

        //No session so assign a guest user and continue to downstream middleware
        if (session == null)
        {
            webContext.User = GuestUser.Instance;
            await _nextFunction(webContext, cancellationToken);
            return;
        }

        var userId = await session.GetValue<string>(UserIdSessionKey);

        //No userId in session so assign a guest user and continue to downstream middleware
        if (userId == null)
        {
            webContext.User = GuestUser.Instance;
            await _nextFunction(webContext, cancellationToken);
            return;
        }

        webContext.User = new AuthenticatedUser(userId);

        await _nextFunction(webContext, cancellationToken);
    }
}
