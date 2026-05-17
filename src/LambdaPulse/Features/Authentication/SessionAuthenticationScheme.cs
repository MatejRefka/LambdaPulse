using LambdaPulse.Engine.Features.Authentication.Abstractions;
using LambdaPulse.Engine.Http.Abstractions;

namespace LambdaPulse.Engine.Features.Authentication;

internal sealed class SessionAuthenticationScheme : IAuthenticationScheme
{
    public string SchemeName => "Session";

    public async Task<IUser> Authenticate(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var session = webContext.Session;

        if (session == null)
        {
            return GuestUser.Instance;
        }

        var userId = await session.GetValue<string>(AuthenticationDefaults.UserIdSessionKey);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return GuestUser.Instance;
        }

        return new AuthenticatedUser(userId);
    }
}
