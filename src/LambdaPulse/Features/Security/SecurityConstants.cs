namespace LambdaPulse.Features.Security;

/// <summary>
/// Constants used for security purposes.
/// </summary>
public static class SecurityConstants
{
    /// <summary>
    /// Session key used to store the CSRF token in the session.
    /// </summary>
    public const string CsrfTokenSessionKey = "csrf.token";
}
