using LambdaPulse.Engine.Http.Abstractions;
using LambdaPulse.Engine.Shared.Extensions;
using System.Security.Cryptography;

namespace LambdaPulse.Engine.Middleware.Implementations;

internal sealed class CsrfMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "CSRF";

    private const string CsrfTokenSessionKey = "csrf.token";

    public CsrfMiddleware(Func<WebContext, CancellationToken, Task> nextFunction) : base(nextFunction)
    {
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var session = webContext.Session;

        //Skip CSRF checks if no session
        if (session == null)
        {
            await _nextFunction(webContext, cancellationToken);
            return;
        }

        var csrfToken = await session.GetValue<string>(CsrfTokenSessionKey);

        //Generate new CSRF token if not in session
        if (string.IsNullOrWhiteSpace(csrfToken))
        {
            //OS-generated random 32-byte token
            csrfToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            await session.SetValue(CsrfTokenSessionKey, csrfToken);
        }

        var method = webContext.WebRequest.Method.ToUpperInvariant();

        //Validate CSRF token for unsafe state-changing requests -POST, PUT, PATCH, DELETE
        if (string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase) || string.Equals(method, "PUT", StringComparison.OrdinalIgnoreCase) || string.Equals(method, "PATCH", StringComparison.OrdinalIgnoreCase) || string.Equals(method, "DELETE", StringComparison.OrdinalIgnoreCase))
        {
            webContext.WebRequest.Headers.TryGetValue("X-CSRF-Token", out var requestCsrfToken);

            //request CSRF token has not been sent or does not match session's CSRF token
            if (string.IsNullOrWhiteSpace(requestCsrfToken) || !CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(csrfToken), Convert.FromBase64String(requestCsrfToken)))
            {
                webContext.WebResponse.StatusCode = 403;
                webContext.WebResponse.ResponsePhrase = "Forbidden";
                await webContext.WebResponse.WriteStringToBody("CSRF token missing or invalid.", cancellationToken);
                return;
            }
        }

        await _nextFunction(webContext, cancellationToken);

        //Set/re-set CSRF token header for client to use in future requests
        webContext.WebResponse.Headers["X-CSRF-Token"] = csrfToken;
    }
}
