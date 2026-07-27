using LambdaPulse.Configuration;
using LambdaPulse.Http.Abstractions;

namespace LambdaPulse.Features.State.Sessions;

internal sealed class PreSessionInitializer : IPreSessionInitializer
{
    private readonly bool _cookieSecure;

    public PreSessionInitializer(IConfigProvider configProvider)
    {
        _cookieSecure = configProvider.ServerConfig.MiddlewareConfig.SessionMiddleware.CookieSecure;
    }

    public void Initialize(WebContext webContext)
    {
        var preSessionCookieExists = false;
        string preSessionToken;

        //check if pre-session cookie exists and is valid
        if (webContext.WebRequest.Cookies.TryGetValue(SessionConstants.PreSessionCookieName, out var rawPreSessionToken) && Guid.TryParse(rawPreSessionToken, out var parsedPreSessionToken))
        {
            preSessionCookieExists = true;
            preSessionToken = parsedPreSessionToken.ToString("N");
        }
        else
        {
            preSessionToken = Guid.CreateVersion7(DateTimeOffset.UtcNow).ToString("N");
        }

        webContext.PreSessionToken = preSessionToken;
        webContext.Trace.PreSessionToken = preSessionToken;

        //set pre-session cookie if it doesn't exist
        if (!preSessionCookieExists)
        {
            var secureAttribute = _cookieSecure ? "; Secure" : string.Empty;
            webContext.WebResponse.Cookies.Add($"{SessionConstants.PreSessionCookieName}={preSessionToken}; Path=/; SameSite=Lax; Max-Age={SessionConstants.PreSessionCookieMaxAgeSeconds}{secureAttribute}");
        }
    }
}
