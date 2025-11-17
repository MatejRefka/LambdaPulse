using LambdaPulse.Configuration;
using LambdaPulse.DI;
using LambdaPulse.Middleware;
using LambdaPulse.Middleware.Implementations;
using LambdaPulse.Services.Http;
using System.Net.Sockets;

namespace LambdaPulse.Services;

public sealed class ClientHandler : IClientHandler
{
    private readonly DependencyResolver _dependencyResolver;
    private readonly IRequestReader _requestReader;
    private readonly IRequestParser _requestParser;
    private readonly IResponseWriter _responseWriter;
    private readonly int _connectionIdleTimeoutMS;

    public ClientHandler(DependencyResolver dependencyResolver, IRequestReader requestReader, IRequestParser requestParser, IResponseWriter responseWriter, IConfigProvider configProvider)
    {
        _dependencyResolver = dependencyResolver;
        _requestReader = requestReader;
        _requestParser = requestParser;
        _responseWriter = responseWriter;
        _connectionIdleTimeoutMS = configProvider.ServerConfig.MiddlewareConfig.ConnectionIdleTimeoutMS;
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
            var pipeline = new Pipeline(_dependencyResolver)
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
                connectionIdleCTS.CancelAfter(_connectionIdleTimeoutMS);
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
                    await _responseWriter.WriteHttpResponse(networkStream, webContext);

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
        catch (Exception e)
        {
            //Unexpected fatal connection error
            Console.WriteLine(e.InnerException);
        }
    }
}