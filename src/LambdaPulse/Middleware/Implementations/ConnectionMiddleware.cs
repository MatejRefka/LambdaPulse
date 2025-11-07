using LambdaPulse.Configuration.Models;
using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Middleware.Implementations;

/// <summary>
/// Handles connection-level control headers of WebRequest and WebResponse.
/// Enforces request execution timeout set by the server's configuratiom via a request-level CTS which is passed to downstream middleware.
/// </summary>
public sealed class Connection : MiddlewareBase
{
    private readonly Config _config;
    public Connection(Func<WebContext, CancellationToken, Task> nextFunction, Config config) : base(nextFunction)
    {
        _nextFunction = nextFunction;
        _config = config;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        //request-level token. sets timeout for long mw execution + response write
        using var requestTimeoutCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        requestTimeoutCTS.CancelAfter(_config.ServerConfig.MiddlewareConfig.RequestExecutionTimeoutMS);

        try
        {
            await _nextFunction(webContext, requestTimeoutCTS.Token);

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
            }
        }
    }
}
