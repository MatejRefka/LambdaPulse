namespace LambdaPulse.Features.State.Sessions;

/// <summary>
/// Constants used for session management purposes.
/// </summary>
public static class SessionConstants
{
    /// <summary>
    /// Name of the cookie storing the session ID.
    /// </summary>
    public const string SessionCookieName = "LambdaPulse.Session";

    /// <summary>
    /// Name of the cookie storing the pre-session ID, used for anonymous sessions.
    /// </summary>
    public const string PreSessionCookieName = "LambdaPulse.PreSession";

    /// <summary>
    /// Name of the cookie storing the anonymous session ID, used for anonymous sessions.
    /// </summary>
    public const string AnonymousSessionCookieName = "LambdaPulse.AnonymousSession";

    /// <summary>
    /// Maximum age of the pre-session cookie in seconds (10 minutes).
    /// </summary>
    public const int PreSessionCookieMaxAgeSeconds = 600;
}
