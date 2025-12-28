using LambdaPulse.Configuration;
using LambdaPulse.Services;
using System.Collections.Concurrent;

namespace LambdaPulse;

public class WebServer : IDisposable
{
    private readonly IListener _listener;
    private readonly IClientHandler _clientHandler;
    private readonly int _backlog;
    //multiple threads can be adding/removing tasks concurrently. Byte is a dummy value
    private readonly ConcurrentDictionary<Task, byte> _activeConnections = new();
    private readonly CancellationTokenSource _serverCancellationSource = new();
    private bool _disposed;

    public WebServer(IListener listener, IClientHandler clientHandler, IConfigProvider configProvider)
    {
        //the contained TcpLister is application-level listener
        _listener = listener;

        _clientHandler = clientHandler;
        _backlog = configProvider.ServerConfig.BackLog;
    }

    public async Task StartServer()
    {
        //OS creates a socket in LISTEN state
        _listener.Start(_backlog);

        //listen forever, continuously accepting clients
        while (true)
        {
            try
            {
                //wait for OS to complete TCP handshake. TcpClient holds layer 4 connection (source IP+port, dest IP+port)
                var tcpClient = await _listener.AcceptTcpClientAsync(_serverCancellationSource.Token);

                //handle each client on a background thread
                var clientTask = _clientHandler.HandleClient(tcpClient, _serverCancellationSource.Token);

                _activeConnections.TryAdd(clientTask, 0);

                //remove completed task from active connections. Do not await here to avoid blocking
                clientTask.ContinueWith(task =>
                {
                    if (task.Exception != null)
                    {
                        //log
                    }
                    _activeConnections.TryRemove(task, out _);
                });
            }
            catch (OperationCanceledException)
            {
                //AcceptTcpClientAsync throws when cancellation is requested, i.e. StopServer() is called
                //break the loop to stop the server from accepting new connections
                break;
            }
            catch (Exception ex)
            {
                //Listener/socket/OS error. Single connection failure should not take out the whole server
                Console.WriteLine(ex.ToString());
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
