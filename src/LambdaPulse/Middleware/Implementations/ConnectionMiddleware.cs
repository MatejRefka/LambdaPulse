using LambdaPulse.Configuration.Models;
using LambdaPulse.Services.Http.Models;
using System.Net.Sockets;

namespace LambdaPulse.Middleware.Implementations;

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
        using var connectionCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        connectionCTS.CancelAfter(_config.ServerConfig.MiddlewareConfig.ConnectionIdleTimeoutMS);

        try
        {
            await _nextFunction(webContext, connectionCTS.Token);

            //client requests connection close
            if (webContext.WebRequest.Headers.TryGetValue("Connection", out var connection) && connection.Equals("close", StringComparison.OrdinalIgnoreCase))
            {
                webContext.WebResponse.Headers["Connection"] = "close";
            }
            else
            {
                webContext.WebResponse.Headers["Connection"] = "keep-alive";
            }
        }
        catch (OperationCanceledException)
        {
            if (!webContext.WebResponse.StatusCode.HasValue && !webContext.WebResponse.StatusCode.HasValue)
            {
                webContext.WebResponse.StatusCode = 408;
                webContext.WebResponse.ResponsePhrase = "Connection timed out.";
            }
        }
        catch (IOException)
        {
            //graceful connection disconnect (browser tab closed -> FIN sent,...)
        }
        catch (SocketException)
        {
            //forceful connection disconnect (network drop -> TCP/IP RST flag sent,...)
        }
    }
}
