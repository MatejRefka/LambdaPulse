using LambdaPulse.Server.Configuration;
using LambdaPulse.Server.Http.Abstractions;
using System.Globalization;

namespace LambdaPulse.Server.Middleware.Implementations;

internal sealed class CorsMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "CORS";

    private readonly HashSet<string> _allowedOrigins;
    private readonly bool _allowCredentials;
    private readonly HashSet<string> _exposedHeaders;
    private readonly HashSet<string> _allowedMethods;
    private readonly HashSet<string> _allowedHeaders;
    private readonly int _preflightMaxAgeSeconds;
    public CorsMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, IConfigProvider configProvider) : base(nextFunction)
    {
        _allowedOrigins = configProvider.ServerConfig.MiddlewareConfig.CorsMiddleware.AllowedOrigins;
        _allowCredentials = configProvider.ServerConfig.MiddlewareConfig.CorsMiddleware.AllowCredentials;
        _exposedHeaders = configProvider.ServerConfig.MiddlewareConfig.CorsMiddleware.ExposedHeaders;
        _allowedMethods = configProvider.ServerConfig.MiddlewareConfig.CorsMiddleware.AllowedMethods;
        _allowedHeaders = configProvider.ServerConfig.MiddlewareConfig.CorsMiddleware.AllowedHeaders;
        _preflightMaxAgeSeconds = configProvider.ServerConfig.MiddlewareConfig.CorsMiddleware.PreflightMaxAgeSeconds;
        _nextFunction = nextFunction;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        webContext.WebRequest.Headers.TryGetValue("Origin", out var origin);

        //same-origin or non-browser request
        if (string.IsNullOrWhiteSpace(origin))
        {
            await _nextFunction(webContext, cancellationToken);
            return;
        }

        //origin is not within the allowed list
        if (!_allowedOrigins.Contains(origin))
        {
            await _nextFunction(webContext, cancellationToken);
            return;
        }

        //allow sending to origin
        webContext.WebResponse.Headers["Access-Control-Allow-Origin"] = origin;

        //add "Origin" to Vary header. Cache now needs to check request Origin before serving cached content
        webContext.WebResponse.Headers.TryGetValue("Vary", out var varyHeaderValue);
        if (string.IsNullOrWhiteSpace(varyHeaderValue))
        {
            webContext.WebResponse.Headers["Vary"] = "Origin";
        }
        else if (!varyHeaderValue.Contains("Origin", StringComparison.OrdinalIgnoreCase))
        {
            webContext.WebResponse.Headers["Vary"] = $"{varyHeaderValue}, Origin";
        }

        //browser-stored credentials are sent with the request (session cookies, http auth,...)
        if (_allowCredentials)
        {
            webContext.WebResponse.Headers["Access-Control-Allow-Credentials"] = "true";
        }

        //allow JS to read these headers
        if (_exposedHeaders.Count > 0)
        {
            webContext.WebResponse.Headers["Access-Control-Expose-Headers"] = string.Join(", ", _exposedHeaders);
        }

        //preflight request
        if (string.Equals(webContext.WebRequest.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase) && webContext.WebRequest.Headers.ContainsKey("Access-Control-Request-Method"))
        {
            //methods that are allowed when making cross-origin requests
            if (_allowedMethods.Count > 0)
            {
                webContext.WebResponse.Headers["Access-Control-Allow-Methods"] = string.Join(", ", _allowedMethods);
            }
            //browser blocks requests containing headers outside of this list + its small set of default headers
            if (_allowedHeaders.Count > 0)
            {
                webContext.WebResponse.Headers["Access-Control-Allow-Headers"] = string.Join(", ", _allowedHeaders);
            }
            //how long the browser should cache the OPTIONS response.
            if (_preflightMaxAgeSeconds > 0)
            {
                webContext.WebResponse.Headers["Access-Control-Max-Age"] = _preflightMaxAgeSeconds.ToString(CultureInfo.InvariantCulture);
            }

            webContext.WebResponse.StatusCode = 204;
            webContext.WebResponse.ResponsePhrase = "Preflight response";
            return;
        }

        await _nextFunction(webContext, cancellationToken);
    }
}
