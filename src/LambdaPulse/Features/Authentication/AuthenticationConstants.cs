namespace LambdaPulse.Features.Authentication;

/// <summary>
/// Constants used for authentication purposes.
/// </summary>
public static class AuthenticationConstants
{
    /// <summary>
    /// Session key used to store the authenticated user's ID in the session.
    /// </summary>
    public const string UserIdSessionKey = "auth.user_id";

    /// <summary>
    /// Dummy password hash to prevent timing attacks during authentication.
    /// </summary>
    public const string DummyPasswordHash = "$2a$12$C.L6Jk3FzCgeuH5Y5iQW6OFJBCm5ulCkMZKM1J7ZIErS4YK9b.I6G";
}
