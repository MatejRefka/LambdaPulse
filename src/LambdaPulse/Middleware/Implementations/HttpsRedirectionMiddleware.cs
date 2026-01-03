using LambdaPulse.Server.Configuration;
using LambdaPulse.Server.Services.Http.Models;
using LambdaPulse.Server.Utility.Extensions;

namespace LambdaPulse.Server.Middleware.Implementations;

/// <summary>
/// Constructs a redirect response for http requests 
/// </summary>
public sealed class HttpsRedirectionMiddleware : MiddlewareBase
{
    private readonly bool _httpsRedirectionEnabled;
    public HttpsRedirectionMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, IConfigProvider configProvider) : base(nextFunction)
    {
        _nextFunction = nextFunction;
        _httpsRedirectionEnabled = configProvider.ServerConfig.MiddlewareConfig.HttpsRedirectionEnabled;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        //https redirect disabled by server or request is https -forwarded by reverse proxy
        if (!_httpsRedirectionEnabled || (webContext.WebRequest.Headers.TryGetValue("X-Forwarded-Proto", out var fwProtocol) && string.Equals(fwProtocol, "https", StringComparison.OrdinalIgnoreCase)))
        {
            await _nextFunction(webContext, cancellationToken);
            return;
        }

        //Host header has not been sent
        if (!webContext.WebRequest.Headers.TryGetValue("Host", out var hostHeaderValue))
        {
            webContext.WebResponse.StatusCode = 400;
            webContext.WebResponse.ResponsePhrase = "Bad Request";
            await webContext.WebResponse.WriteToBody("Missing Host Header");

            webContext.WebResponse.HasStarted = true;
            return;
        }
        else
        {
            //strip out port if included 
            var portStartIndex = hostHeaderValue?.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
            var hostNoPort = (portStartIndex != null && portStartIndex > 0) ? hostHeaderValue?[..(int)portStartIndex] : hostHeaderValue;

            //default port is 443 if not specified
            var redirectUrl = $"https://{hostNoPort}{webContext.WebRequest.Path}";

            webContext.WebResponse.StatusCode = 307;
            webContext.WebResponse.ResponsePhrase = "Internal Redirect";
            webContext.WebResponse.Headers["Location"] = redirectUrl;
            if (!webContext.WebRequest.Headers.ContainsKey("X-Forwarded-For"))
            {
                webContext.WebResponse.Headers["Connection"] = "close";
            }

            webContext.WebResponse.HasStarted = true;
        }
    }
}
