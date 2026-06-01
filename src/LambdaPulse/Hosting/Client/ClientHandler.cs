using LambdaPulse.Engine.Configuration;
using LambdaPulse.Engine.Features.Logging;
using LambdaPulse.Engine.Http.Abstractions;
using LambdaPulse.Engine.Http.Parsing;
using LambdaPulse.Engine.Http.Reading;
using LambdaPulse.Engine.Http.Writing;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LambdaPulse.Engine.Hosting.Client;

internal sealed class ClientHandler : IClientHandler
{
    private readonly Func<WebContext, CancellationToken, Task> _pipeline;
    private readonly IRequestReader _requestReader;
    private readonly IRequestParser _requestParser;
    private readonly IResponseWriter _responseWriter;
    private readonly IEngineLogger _engineLogger;
    private readonly ITraceLogger _traceLogger;
    private readonly int _readTimeoutMS;

    public ClientHandler(Func<WebContext, CancellationToken, Task> pipeline, IRequestReader requestReader, IRequestParser requestParser, IResponseWriter responseWriter, IConfigProvider configProvider, IEngineLogger engineLogger, ITraceLogger traceLogger)
    {
        _pipeline = pipeline;
        _requestReader = requestReader;
        _requestParser = requestParser;
        _responseWriter = responseWriter;
        _engineLogger = engineLogger;
        _traceLogger = traceLogger;
        _readTimeoutMS = configProvider.ServerConfig.ReadTimeoutMS;
    }

    public async Task HandleClient(TcpClient tcpClient, CancellationToken serverCancellationToken = default)
    {
        //client-level token
        using var clientCts = CancellationTokenSource.CreateLinkedTokenSource(serverCancellationToken);
        var clientCancellationToken = clientCts.Token;

        try
        {
            //abstraction for reading and sending bytes over the TCP connection
            await using var networkStream = tcpClient.GetStream();

            //remote IP to be passed to WebContext
            var remoteIpAddress = (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString();

            //keep accepting requests over the same connection
            while (!clientCancellationToken.IsCancellationRequested)
            {
                //connection-level token. sets idle timeout between requests
                using var readTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(clientCancellationToken);
                readTimeoutCts.CancelAfter(_readTimeoutMS);
                var readTimeoutToken = readTimeoutCts.Token;

                string? requestString = null;
                try
                {
                    requestString = await _requestReader.ReadHttpRequest(networkStream, readTimeoutToken);

                    //client disconnected gracefully before sending anything
                    if (string.IsNullOrWhiteSpace(requestString))
                    {
                        break;
                    }

                    //start timestamp of the request (Trace)
                    var requestStartTimestamp = DateTimeOffset.UtcNow;
                    var timer = new System.Diagnostics.Stopwatch();
                    timer.Start();

                    WebContext webContext;

                    try
                    {
                        webContext = _requestParser.ParseHttpRequest(requestString, requestStartTimestamp, remoteIpAddress);
                    }
                    catch (Exception e)
                    {
                        timer.Stop();

                        _engineLogger.Log(LogLevel.Warning, "ClientHandler", "Malformed HTTP request.", e);

                        var trace = new Trace
                        {
                            TimestampStart = requestStartTimestamp,
                            DurationMs = timer.ElapsedMilliseconds,
                            ResponseStatusCode = 400,
                            ResponsePhrase = "Bad request"
                        };

                        _traceLogger.Log(trace);

                        await _responseWriter.WriterRaw400Response(networkStream, clientCancellationToken);

                        break;
                    }

                    //invoke the delegate
                    await _pipeline(webContext, clientCancellationToken);

                    await _responseWriter.WriteHttpResponse(networkStream, webContext, clientCancellationToken);

                    timer.Stop();
                    //log the request + response metadata (Trace)
                    webContext.Trace.DurationMs = timer.ElapsedMilliseconds;
                    webContext.Trace.ResponseStatusCode = webContext.WebResponse.StatusCode;
                    webContext.Trace.ResponsePhrase = webContext.WebResponse.ResponsePhrase;
                    webContext.Trace.ResponseHeaders = webContext.WebResponse.Headers;
                    webContext.Trace.ResponseCookies = webContext.WebResponse.Cookies;
                    webContext.Trace.ResponseBody = webContext.WebResponse.Body != null ? Encoding.UTF8.GetString(webContext.WebResponse.Body.ToArray()) : null;

                    _traceLogger.Log(webContext.Trace);

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
                catch (Exception e)
                {
                    //Unexpected error within the request. Terminate the connection for safety.
                    _engineLogger.Log(LogLevel.Error, "ClientHandler", "Unexpected error while processing request.", e);
                    break;
                }
            }
            //Connection is closed here
        }
        catch (Exception e)
        {
            //Unexpected fatal connection error
            _engineLogger.Log(LogLevel.Error, "ClientHandler", "Unexpected fatal connection error.", e);
        }
    }
}