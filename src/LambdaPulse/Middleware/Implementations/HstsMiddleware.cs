using LambdaPulse.Server.Configuration;
using LambdaPulse.Server.Http.Abstractions;

namespace LambdaPulse.Server.Middleware.Implementations;
/// <summary>
/// Adds Strict-Transport-Security header on server responses. This instructs the browser never use HTTP for the set domain.
/// Any HTTP requests to the server are upgraded to HTTPS by the browser before sending. Server must be configured for HTTPS.
/// </summary>
internal sealed class HstsMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "HSTS";

    private readonly int _maxAge;
    private readonly bool _includeSubDomains;
    private readonly bool _preload;
    public HstsMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, IConfigProvider configProvider) : base(nextFunction)
    {
        _nextFunction = nextFunction;
        _maxAge = configProvider.ServerConfig.MiddlewareConfig.HstsMiddleware.MaxAge;
        _includeSubDomains = configProvider.ServerConfig.MiddlewareConfig.HstsMiddleware.IncludeSubDomains;
        _preload = configProvider.ServerConfig.MiddlewareConfig.HstsMiddleware.Preload;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        await _nextFunction(webContext, cancellationToken);

        //HSTS only applies to HTTPS
        if (!(webContext.WebRequest.Headers.TryGetValue("X-Forwarded-Proto", out var fwProtocol) && string.Equals(fwProtocol, "https", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var hstsHeaderValue = $"max-age={_maxAge}";
        if (_includeSubDomains)
        {
            hstsHeaderValue += "; includeSubDomains";
        }
        if (_preload)
        {
            hstsHeaderValue += "; preload";
        }

        webContext.WebResponse.Headers["Strict-Transport-Security"] = hstsHeaderValue;
    }
}
