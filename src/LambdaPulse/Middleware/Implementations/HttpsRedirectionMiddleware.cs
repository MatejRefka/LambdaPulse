using LambdaPulse.Configuration;
using LambdaPulse.Features.Logging;
using LambdaPulse.Http.Abstractions;
using LambdaPulse.Shared.Extensions;

namespace LambdaPulse.Middleware.Implementations;

/// <summary>
/// Constructs a redirect response for http requests if the server is configured to enforce https.
/// Skips redirection if the request is forwarded as https by a reverse proxy
/// </summary>
internal sealed class HttpsRedirectionMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "HTTPS";

    private readonly bool _isEnabled;
    public HttpsRedirectionMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, Config config) : base(nextFunction)
    {
        _isEnabled = config.ServerConfig.MiddlewareConfig.HttpsRedirectionConfig.IsEnabled;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var downstreamStart = DateTimeOffset.UtcNow;

        //https redirect disabled by server or request is https -forwarded by reverse proxy
        if (!_isEnabled || (webContext.WebRequest.Headers.TryGetValue("X-Forwarded-Proto", out var fwProtocol) && string.Equals(fwProtocol, "https", StringComparison.OrdinalIgnoreCase)))
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { !_isEnabled ? "HTTPS redirection disabled by server." : "Request is forwarded as HTTPS. Skip HTTPS redirection.", });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //Host header has not been sent
        if (!webContext.WebRequest.Headers.TryGetValue("Host", out var hostHeaderValue))
        {
            webContext.WebResponse.StatusCode = 400;
            webContext.WebResponse.ResponsePhrase = "Bad Request";
            await webContext.WebResponse.WriteStringToBody("Missing Host Header.", cancellationToken);

            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.ShortCircuit, downstreamStart, new List<string> { "Host header is required for HTTPS redirection. Return 400." });
            return;
        }
        else
        {
            //strip out port if included 
            var portStartIndex = hostHeaderValue?.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
            var hostNoPort = (portStartIndex != null && portStartIndex > 0) ? hostHeaderValue?[..(int)portStartIndex] : hostHeaderValue;
            var queryString = string.Join("&", webContext.WebRequest.QueryParameters.Select(param => $"{param.Key}={param.Value}"));

            //default port is 443 if not specified
            var querySuffix = string.IsNullOrWhiteSpace(queryString) ? string.Empty : $"?{queryString}";
            var redirectUrl = $"https://{hostNoPort}{webContext.WebRequest.Path}{querySuffix}";

            webContext.WebResponse.StatusCode = 307;
            webContext.WebResponse.ResponsePhrase = "Temporary Redirect";
            webContext.WebResponse.Headers["Location"] = redirectUrl;

            //client will open a new connection to the redirect url, so close the current connection
            webContext.WebResponse.Headers["Connection"] = "close";
            webContext.ConnectionCloseRequested = true;

            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.ShortCircuit, downstreamStart, new List<string> { $"Redirect to HTTPS. location={redirectUrl}.", "Client must open a new HTTPS request. Close connection." });
        }
    }
}
