namespace LambdaPulse.Features.Authentication;

/// <summary>
/// Represents an authenticated user within an IAuthenticationScheme.
/// </summary>
public sealed class AuthenticatedUser : IUser
{
    /// <summary>
    /// Unique id of the user.
    /// </summary>
    public string? Id { get; }

    /// <summary>
    /// Indicates whether the user is authenticated.
    /// </summary>
    public bool IsAuthenticated => true;

    /// <summary>
    /// Roles granted to the user.
    /// </summary>
    public HashSet<string> Roles { get; }

    /// <summary>
    /// Initializes a new instance of the AuthenticatedUser class
    /// </summary>
    public AuthenticatedUser(string id, HashSet<string>? roles = null)
    {
        Id = id;
        Roles = (roles != null) ? new HashSet<string>(roles, StringComparer.OrdinalIgnoreCase) : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }
}
