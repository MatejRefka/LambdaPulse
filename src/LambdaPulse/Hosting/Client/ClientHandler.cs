using LambdaPulse.Configuration.Models;
using LambdaPulse.Middleware;
using LambdaPulse.Middleware.Implementations;
using LambdaPulse.Services.Http;
using System.Net.Sockets;

namespace LambdaPulse.Services;

public sealed class ClientHandler : IClientHandler
{
    private readonly IRequestReader _requestReader;
    private readonly IRequestParser _requestParser;
    private readonly IResponseWriter _responseWriter;
    private readonly Config _config;

    public ClientHandler(IRequestReader requestReader, IRequestParser requestParser, IResponseWriter responseWriter, Config config)
    {
        _requestReader = requestReader;
        _requestParser = requestParser;
        _responseWriter = responseWriter;
        _config = config;
    }

    public async Task HandleClient(TcpClient tcpClient, CancellationToken serverCancellationToken)
    {
        //client-level token
        using var clientCTS = CancellationTokenSource.CreateLinkedTokenSource(serverCancellationToken);
        var clientCancellationToken = clientCTS.Token;

        try
        {
            //abstraction for reading and sending bytes over the TCP connection
            await using var networkStream = tcpClient.GetStream();

            //construct the middleware pipeline, once per connection
            var pipeline = new Pipeline()
                            .AddMiddleware<ExceptionHandler>()
                            .AddMiddleware<Logging>()
                            .AddMiddleware<RequestLimits>()
                            .AddMiddleware<Connection>()
                            .AddMiddleware<HttpsRedirection>()
                            .AddMiddleware<HSTS>()
                            .AddMiddleware<Security>()
                            .AddMiddleware<State>()
                            .AddMiddleware<CSRF>()
                            .AddMiddleware<StaticFiles>()
                            .AddMiddleware<ResponseCompression>()
                            .AddMiddleware<Routing>()
                            .AddMiddleware<CORS>()
                            .AddMiddleware<Authentication>()
                            .AddMiddleware<Authorization>()
                            .AddMiddleware<ContentNegotiation>()
                            .AddMiddleware<Endpoint>()
                            .Build();

            //keep accepting requests over the same connection
            while (!clientCancellationToken.IsCancellationRequested)
            {
                //request-level token. sets idle timeout between requests
                using var connectionIdleCTS = CancellationTokenSource.CreateLinkedTokenSource(clientCancellationToken);
                connectionIdleCTS.CancelAfter(_config.ServerConfig.MiddlewareConfig.ConnectionIdleTimeoutMS);
                var timeoutToken = connectionIdleCTS.Token;

                string? requestString = null;
                try
                {
                    requestString = await _requestReader.ReadHttpRequest(networkStream, timeoutToken);

                    //client disconnected gracefully before sending anything
                    if (string.IsNullOrEmpty(requestString))
                    {
                        break;
                    }

                    var webContext = _requestParser.ParseHttpRequest(requestString);

                    //Invoke the delegate
                    await pipeline(webContext, timeoutToken);

                    webContext.WebResponse.HasStarted = true;
                    await _responseWriter.WriteHttpResponse(networkStream, webContext.WebResponse);

                    //connection middleware flags connection close
                    if (webContext.ConnectionCloseRequested)
                    {
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    //connection disconnect - idle timeout reached
                    break;
                }
                catch (IOException)
                {
                    //graceful connection disconnect (browser tab closed -> FIN sent,...)
                    break;
                }
                catch (SocketException)
                {
                    //forceful connection disconnect (network drop -> TCP/IP RST flag sent,...)
                    break;
                }
                catch (Exception)
                {
                    //Unexpected error within the request
                    //Continue serving other requests, do not break the connection
                }
            }
            //Connection is closed here
        }
        catch (Exception)
        {
            //Unexpected fatal connection error
        }
    }
}