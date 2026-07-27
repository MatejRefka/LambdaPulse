namespace LambdaPulse.Features.State.Sessions;

public static class SessionConstants
{
    public const string SessionCookieName = "LambdaPulse.Session";
    public const string PreSessionCookieName = "LambdaPulse.PreSession";
    public const string AnonymousSessionCookieName = "LambdaPulse.AnonymousSession";
    public const int PreSessionCookieMaxAgeSeconds = 600;
}
