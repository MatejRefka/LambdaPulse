using LambdaPulse.Server.Http.Abstractions;
using LambdaPulse.Server.Shared.Extensions;
using System.Security.Cryptography;

namespace LambdaPulse.Server.Middleware.Implementations;

public sealed class CsrfMiddleware : MiddlewareBase
{
    private const string CsrfTokenSessionKey = "csrf.token";

    public CsrfMiddleware(Func<WebContext, CancellationToken, Task> nextFunction) : base(nextFunction)
    {
        _nextFunction = nextFunction;
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
        if (string.IsNullOrEmpty(csrfToken))
        {
            //OS-generated random 32-byte token
            csrfToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            await session.SetValue(CsrfTokenSessionKey, csrfToken);
        }

        var method = webContext.WebRequest.Method.ToUpperInvariant();

        //Validate CSRF token for unsafe state-changing requests
        if (method == "POST" || method == "PUT" || method == "PATCH" || method == "DELETE")
        {
            webContext.WebRequest.Headers.TryGetValue("X-CSRF-Token", out var requestCsrfToken);

            //request CSRF token has not been sent or does not match session's CSRF token
            if (string.IsNullOrEmpty(requestCsrfToken) || !CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(csrfToken), Convert.FromBase64String(requestCsrfToken)))
            {
                webContext.WebResponse.StatusCode = 403;
                webContext.WebResponse.ResponsePhrase = "Forbidden.";
                await webContext.WebResponse.WriteStringToBody("CSRF token missing or invalid.", cancellationToken);
                return;
            }
        }

        await _nextFunction(webContext, cancellationToken);

        //Set/re-set CSRF token header for client to use in future requests
        webContext.WebResponse.Headers["X-CSRF-Token"] = csrfToken;
    }
}
