namespace LambdaPulse.Features.Authentication;

/// <summary>
/// User within the IAuthenticationScheme.
/// </summary>
public interface IUser
{
    /// <summary>
    /// Unique id of the user.
    /// </summary>
    string? Id { get; }

    /// <summary>
    /// Indicates whether the user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Roles granted to the user.
    /// </summary>
    HashSet<string> Roles { get; }
}
