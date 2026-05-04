using LambdaPulse.Engine.Configuration;
using LambdaPulse.Engine.Features.Logging;
using LambdaPulse.Engine.Hosting.Client;
using LambdaPulse.Engine.Hosting.Connection;
using System.Collections.Concurrent;

namespace LambdaPulse.Engine.Hosting;

internal sealed class WebServer : IWebServer
{
    private readonly IConnectionListener _listener;
    private readonly IClientHandler _clientHandler;
    private readonly int _backlog;
    private readonly IEngineLogger _engineLogger;
    //multiple threads can be adding/removing tasks concurrently. Byte is a dummy value
    private readonly ConcurrentDictionary<Task, byte> _activeConnections = new();
    private readonly CancellationTokenSource _serverCancellationSource = new();
    private bool _disposed;

    public WebServer(IConnectionListener listener, IClientHandler clientHandler, IConfigProvider configProvider, IEngineLogger engineLogger)
    {
        //the contained TcpLister is application-level listener
        _listener = listener;

        _clientHandler = clientHandler;
        _backlog = configProvider.ServerConfig.BackLog;
        _engineLogger = engineLogger;
    }

    public async Task StartServer()
    {
        //OS creates a socket in LISTEN state
        _listener.Start(_backlog);
        _engineLogger.Log(LogLevel.Info, "WebServer", "Server started. Listening for incoming connections...");

        //listen forever, continuously accepting clients
        while (true)
        {
            try
            {
                //wait for OS to complete TCP handshake. TcpClient holds layer 4 connection (source IP+port, dest IP+port)
                var tcpClient = await _listener.AcceptTcpClientAsync(_serverCancellationSource.Token);
                var remoteEndPoint = tcpClient.Client.RemoteEndPoint;

                //handle each client on a background thread
                var clientTask = _clientHandler.HandleClient(tcpClient, _serverCancellationSource.Token);

                _activeConnections.TryAdd(clientTask, 0);

#pragma warning disable CS4014
                //remove completed task from active connections. Do not await here to avoid blocking
                clientTask.ContinueWith(task =>
                {
                    if (task.Exception != null)
                    {
                        _engineLogger.Log(LogLevel.Error, "WebServer", $"[{remoteEndPoint}] Connection error.", task.Exception);
                    }
                    _activeConnections.TryRemove(task, out _);
                });
#pragma warning restore CS4014
            }
            catch (OperationCanceledException)
            {
                //AcceptTcpClientAsync throws when cancellation is requested, i.e. StopServer() is called
                _engineLogger.Log(LogLevel.Info, "WebServer", "Server is shutting down. No longer accepting new connections.");

                //break the loop to stop the server from accepting new connections
                break;
            }
            catch (Exception e)
            {
                //Listener/socket/OS error. Single connection failure should not take out the whole server
                _engineLogger.Log(LogLevel.Error, "WebServer", "Critical error accepting incoming connection.", e);
            }
        }

        //wait for all active connections to complete before shutting down the server
        await Task.WhenAll(_activeConnections.Keys);
    }

    public async Task StopServer()
    {
        if (!_disposed)
        {
            _serverCancellationSource.Cancel();
            _listener.Stop();

            //wait for all active connections to complete before shutting down the server
            await Task.WhenAll(_activeConnections.Keys);

            _serverCancellationSource.Dispose();
            _disposed = true;

            _engineLogger.Log(LogLevel.Info, "WebServer", "Server shutdown complete.");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _serverCancellationSource.Cancel();
            _listener.Stop();
            _serverCancellationSource.Dispose();

            _disposed = true;
        }
        //prevents GC from calling Object.Finalize (redundant) when a destructor is declared
        GC.SuppressFinalize(this);
    }
}
