using LambdaPulse.Http.Abstractions;

namespace LambdaPulse.Features.Authentication;

/// <summary>
/// Authentication scheme used by the AuthenticationMiddleware to authenticate incoming requests.
/// </summary>
public interface IAuthenticationScheme
{
    /// <summary>
    /// Name of the authentication scheme.
    /// </summary>
    string SchemeName { get; }

    /// <summary>
    /// Authenticates the incoming request and returns the authenticated user.
    /// </summary>
    Task<IUser> Authenticate(WebContext webContext, CancellationToken cancellationToken = default);
}
