using LambdaPulse.Engine.Features.Logging;
using LambdaPulse.Engine.Features.Security;
using LambdaPulse.Engine.Http.Abstractions;
using LambdaPulse.Engine.Shared.Extensions;
using System.Security.Cryptography;

namespace LambdaPulse.Engine.Middleware.Implementations;

/// <summary>
/// Provides Cross-Site Request Forgery protection for state-changing HTTP requests.
/// Validates and issues CSRF tokens using the session state.
/// </summary>
internal sealed class CsrfMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "CSRF";

    public CsrfMiddleware(Func<WebContext, CancellationToken, Task> nextFunction) : base(nextFunction)
    {
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var downstreamStart = DateTimeOffset.UtcNow;
        var logs = new List<string>();

        //Skip CSRF if marked by endpoint (csrf endpoint)
        if (webContext.Endpoint != null && webContext.Endpoint.SkipCsrf)
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "Endpoint is marked to skip CSRF validation. Skip CSRF middleware." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //Short-circuit if no session
        if (webContext.Session == null)
        {
            webContext.WebResponse.StatusCode = 500;
            webContext.WebResponse.ResponsePhrase = "Internal Server Error";
            await webContext.WebResponse.WriteStringToBody("Session is required before CSRF middleware.", cancellationToken);

            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.ShortCircuit, downstreamStart, new List<string> { "CSRF requires session state. Return 500." });
            return;
        }

        var csrfToken = await webContext.Session.GetValue<string>(SecurityConstants.CsrfTokenSessionKey);
        logs.Add(string.IsNullOrWhiteSpace(csrfToken) ? "Session has no CSRF token. Create CSRF token." : "Session CSRF token exists. Reuse CSRF token.");

        //Generate new CSRF token if not in session
        if (string.IsNullOrWhiteSpace(csrfToken))
        {
            //OS-generated random 32-byte token
            csrfToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            await webContext.Session.SetValue(SecurityConstants.CsrfTokenSessionKey, csrfToken);
        }

        var method = webContext.WebRequest.Method;

        //Validate CSRF token for unsafe state-changing requests -POST, PUT, PATCH, DELETE
        if (string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase) || string.Equals(method, "PUT", StringComparison.OrdinalIgnoreCase) || string.Equals(method, "PATCH", StringComparison.OrdinalIgnoreCase) || string.Equals(method, "DELETE", StringComparison.OrdinalIgnoreCase))
        {
            webContext.WebRequest.Headers.TryGetValue("X-CSRF-Token", out var requestCsrfToken);

            //request CSRF token has not been sent or does not match session's CSRF token
            if (!IsTokenValid(requestCsrfToken, csrfToken))
            {
                webContext.WebResponse.StatusCode = 403;
                webContext.WebResponse.ResponsePhrase = "Forbidden";
                await webContext.WebResponse.WriteStringToBody("CSRF token missing or invalid.", cancellationToken);

                logs.Add(string.IsNullOrWhiteSpace(requestCsrfToken) ? "CSRF token is missing." : "CSRF token is invalid.");
                RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.ShortCircuit, downstreamStart, logs);
                return;
            }
            logs.Add("CSRF token is valid. Allow request.");
        }

        RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, logs);

        await _nextFunction(webContext, cancellationToken);

        var upstreamStart = DateTimeOffset.UtcNow;

        //Set/re-set CSRF token header for client to use in future requests
        webContext.WebResponse.Headers["X-CSRF-Token"] = csrfToken;

        RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Set CSRF token. header=X-CSRF-Token." });
    }

    private static bool IsTokenValid(string? requestToken, string? expectedToken)
    {
        if (string.IsNullOrWhiteSpace(requestToken) || string.IsNullOrWhiteSpace(expectedToken))
        {
            return false;
        }

        try
        {
            return CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(requestToken), Convert.FromBase64String(expectedToken));
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
