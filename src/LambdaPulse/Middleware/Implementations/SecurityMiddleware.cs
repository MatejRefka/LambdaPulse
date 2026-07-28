using LambdaPulse.Configuration;
using LambdaPulse.Features.Logging;
using LambdaPulse.Http.Abstractions;

namespace LambdaPulse.Middleware.Implementations;

/// <summary>
/// Adds standard security headers to HTTP response sent back to the browser.
/// </summary>
internal sealed class SecurityMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "Security";

    private readonly bool _xContentTypeOptions;
    private readonly string? _referrerPolicy;
    private readonly string? _permissionsPolicy;
    private readonly string? _crossOriginOpenerPolicy;
    private readonly string? _crossOriginResourcePolicy;
    private readonly string? _crossOriginEmbedderPolicy;
    private readonly bool _removeServerHeader;

    public SecurityMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, Config config) : base(nextFunction)
    {
        _xContentTypeOptions = config.ServerConfig.MiddlewareConfig.SecurityConfig.XContentTypeOptions;
        _referrerPolicy = config.ServerConfig.MiddlewareConfig.SecurityConfig.ReferrerPolicy;
        _permissionsPolicy = config.ServerConfig.MiddlewareConfig.SecurityConfig.PermissionsPolicy;
        _crossOriginOpenerPolicy = config.ServerConfig.MiddlewareConfig.SecurityConfig.CrossOriginOpenerPolicy;
        _crossOriginResourcePolicy = config.ServerConfig.MiddlewareConfig.SecurityConfig.CrossOriginResourcePolicy;
        _crossOriginEmbedderPolicy = config.ServerConfig.MiddlewareConfig.SecurityConfig.CrossOriginEmbedderPolicy;
        _removeServerHeader = config.ServerConfig.MiddlewareConfig.SecurityConfig.RemoveServerHeader;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
        await _nextFunction(webContext, cancellationToken);

        var upstreamStart = DateTimeOffset.UtcNow;
        var logs = new List<string>();

        if (_xContentTypeOptions)
        {
            webContext.WebResponse.Headers["X-Content-Type-Options"] = "nosniff";
            logs.Add("Set X-Content-Type-Options=nosniff.");
        }

        if (!string.IsNullOrWhiteSpace(_referrerPolicy))
        {
            webContext.WebResponse.Headers["Referrer-Policy"] = _referrerPolicy;
            logs.Add($"Set Referrer-Policy={_referrerPolicy}.");
        }

        if (!string.IsNullOrWhiteSpace(_permissionsPolicy))
        {
            webContext.WebResponse.Headers["Permissions-Policy"] = _permissionsPolicy;
            logs.Add($"Set Permissions-Policy={_permissionsPolicy}.");
        }

        if (!string.IsNullOrWhiteSpace(_crossOriginOpenerPolicy))
        {
            webContext.WebResponse.Headers["Cross-Origin-Opener-Policy"] = _crossOriginOpenerPolicy;
            logs.Add($"Set Cross-Origin-Opener-Policy={_crossOriginOpenerPolicy}.");
        }

        if (!string.IsNullOrWhiteSpace(_crossOriginResourcePolicy))
        {
            webContext.WebResponse.Headers["Cross-Origin-Resource-Policy"] = _crossOriginResourcePolicy;
            logs.Add($"Set Cross-Origin-Resource-Policy={_crossOriginResourcePolicy}.");
        }

        if (!string.IsNullOrWhiteSpace(_crossOriginEmbedderPolicy))
        {
            webContext.WebResponse.Headers["Cross-Origin-Embedder-Policy"] = _crossOriginEmbedderPolicy;
            logs.Add($"Set Cross-Origin-Embedder-Policy={_crossOriginEmbedderPolicy}.");
        }

        if (_removeServerHeader && webContext.WebResponse.Headers.ContainsKey("Server"))
        {
            webContext.WebResponse.Headers.Remove("Server");
            logs.Add("Remove response header.");
        }

        RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, logs);
    }
}
