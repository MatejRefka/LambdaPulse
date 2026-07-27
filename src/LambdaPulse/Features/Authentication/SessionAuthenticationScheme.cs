using LambdaPulse.Http.Abstractions;

namespace LambdaPulse.Features.Authentication;

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

        var userId = await session.GetValue<string>(AuthenticationConstants.UserIdSessionKey);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return GuestUser.Instance;
        }

        return new AuthenticatedUser(userId);
    }
}
