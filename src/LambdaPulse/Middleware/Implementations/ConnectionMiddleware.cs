using LambdaPulse.Server.Configuration;
using LambdaPulse.Server.Services.Http.Models;
using LambdaPulse.Server.Utility.Extensions;

namespace LambdaPulse.Server.Middleware.Implementations;

/// <summary>
/// Handles connection-level control headers of WebRequest and WebResponse.
/// Enforces request execution timeout set by the server's configuration via a request-level CTS which is passed to downstream middleware.
/// </summary>
public sealed class ConnectionMiddleware : MiddlewareBase
{
    private readonly int _requestExecutionTimeoutMS;
    public ConnectionMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, IConfigProvider configProvider) : base(nextFunction)
    {
        _nextFunction = nextFunction;
        _requestExecutionTimeoutMS = configProvider.ServerConfig.MiddlewareConfig.RequestExecutionTimeoutMS;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        //request-level token. sets timeout for long mw execution + response write
        using var requestTimeoutCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        requestTimeoutCTS.CancelAfter(_requestExecutionTimeoutMS);

        try
        {
            await _nextFunction(webContext, requestTimeoutCTS.Token);

            //web server behind reverse proxy -do not interfere
            if (webContext.WebRequest.Headers.ContainsKey("X-Forwarded-For"))
            {
                return;
            }

            //downstream middleware already set the connection header
            if (webContext.WebResponse.Headers.ContainsKey("Connection"))
            {
                return;
            }

            //client requests connection close
            if (webContext.WebRequest.Headers.TryGetValue("Connection", out var connectionHeaderValue))
            {
                if (connectionHeaderValue.Equals("close", StringComparison.OrdinalIgnoreCase))
                {
                    webContext.WebResponse.Headers["Connection"] = "close";
                    webContext.ConnectionCloseRequested = true;
                }
                else
                {
                    webContext.WebResponse.Headers["Connection"] = "keep-alive";
                }
            }
            else
            {
                //default to keep-alive if no header specified in request
                webContext.WebResponse.Headers["Connection"] = "keep-alive";
            }
        }
        catch (OperationCanceledException)
        {
            if (!webContext.WebResponse.StatusCode.HasValue && !webContext.WebResponse.HasStarted)
            {
                webContext.WebResponse.StatusCode = 408;
                webContext.WebResponse.ResponsePhrase = "Connection timed out.";
                await webContext.WebResponse.WriteStringToBody("Connection timed out.", cancellationToken);
            }
        }
    }
}
