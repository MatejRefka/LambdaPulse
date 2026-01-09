using LambdaPulse.Server.Configuration;
using LambdaPulse.Server.Http.Abstractions;

namespace LambdaPulse.Server.Middleware.Implementations;

/// <summary>
/// Adds standard security headers to every HTTP response sent back to the browser.
/// </summary>
public sealed class SecurityMiddleware : MiddlewareBase
{
    private readonly bool _xContentTypeOptions;
    private readonly string? _referrerPolicy;
    private readonly string? _permissionsPolicy;
    private readonly string? _crossOriginOpenerPolicy;
    private readonly string? _crossOriginResourcePolicy;
    private readonly string? _crossOriginEmbedderPolicy;
    private readonly bool _removeServerHeader;

    public SecurityMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, IConfigProvider configProvider) : base(nextFunction)
    {
        _nextFunction = nextFunction;
        _xContentTypeOptions = configProvider.ServerConfig.MiddlewareConfig.SecurityMiddleware.XContentTypeOptions;
        _referrerPolicy = configProvider.ServerConfig.MiddlewareConfig.SecurityMiddleware.ReferrerPolicy;
        _permissionsPolicy = configProvider.ServerConfig.MiddlewareConfig.SecurityMiddleware.PermissionsPolicy;
        _crossOriginOpenerPolicy = configProvider.ServerConfig.MiddlewareConfig.SecurityMiddleware.CrossOriginOpenerPolicy;
        _crossOriginResourcePolicy = configProvider.ServerConfig.MiddlewareConfig.SecurityMiddleware.CrossOriginResourcePolicy;
        _crossOriginEmbedderPolicy = configProvider.ServerConfig.MiddlewareConfig.SecurityMiddleware.CrossOriginEmbedderPolicy;
        _removeServerHeader = configProvider.ServerConfig.MiddlewareConfig.SecurityMiddleware.RemoveServerHeader;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        await _nextFunction(webContext, cancellationToken);

        if (_xContentTypeOptions)
        {
            webContext.WebResponse.Headers["X-Content-Type-Options"] = "nosniff";
        }

        if (!string.IsNullOrEmpty(_referrerPolicy))
        {
            webContext.WebResponse.Headers["Referrer-Policy"] = _referrerPolicy;
        }

        if (!string.IsNullOrEmpty(_permissionsPolicy))
        {
            webContext.WebResponse.Headers["Permissions-Policy"] = _permissionsPolicy;
        }

        if (!string.IsNullOrEmpty(_crossOriginOpenerPolicy))
        {
            webContext.WebResponse.Headers["Cross-Origin-Opener-Policy"] = _crossOriginOpenerPolicy;
        }

        if (!string.IsNullOrEmpty(_crossOriginResourcePolicy))
        {
            webContext.WebResponse.Headers["Cross-Origin-Resource-Policy"] = _crossOriginResourcePolicy;
        }

        if (!string.IsNullOrEmpty(_crossOriginEmbedderPolicy))
        {
            webContext.WebResponse.Headers["Cross-Origin-Embedder-Policy"] = _crossOriginEmbedderPolicy;
        }

        if (_removeServerHeader && webContext.WebResponse.Headers.ContainsKey("Server"))
        {
            webContext.WebResponse.Headers.Remove("Server");
        }
    }
}
