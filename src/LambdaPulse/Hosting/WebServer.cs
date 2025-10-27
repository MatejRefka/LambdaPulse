using LambdaPulse.Configuration;
using LambdaPulse.Services;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace LambdaPulse;

public class WebServer
{
    private readonly IClientHandler _clientHandler;
    private readonly TcpListener _listener;
    private readonly int _backlog;
    private readonly ConcurrentBag<Task> _activeConnections = new();
    private readonly CancellationTokenSource _serverCancellationSource = new();

    public WebServer(IClientHandler clientHandler, IConfigProvider configProvider)
    {
        _clientHandler = clientHandler;
        var address = IPAddress.Parse(configProvider.ServerConfig.Address);
        var port = configProvider.ServerConfig.Port;
        _backlog = configProvider.ServerConfig.BackLog;

        //application-level setup
        _listener = new TcpListener(address, port);
    }

    public async Task StartServer()
    {
        //OS creates a socket in LISTEN state
        _listener.Start(_backlog);

        //listen until server-level cancellation is requested, continuously accepting clients
        while (!_serverCancellationSource.IsCancellationRequested)
        {
            try
            {
                //wait for OS to complete TCP handshake. TcpClient holds layer 4 connection (source IP+port, dest IP+port)
                using var tcpClient = await _listener.AcceptTcpClientAsync(_serverCancellationSource.Token);

                //handle each client on a background thread
                var clientTask = _clientHandler.HandleClient(tcpClient, _serverCancellationSource.Token);

                _activeConnections.Add(clientTask);
            }
            catch (OperationCanceledException)
            {
                //stop the server from accepting new connections
                break;
            }
            catch (Exception ex)
            {
                //single client exception should not take out the whole server
                Console.WriteLine(ex.ToString());
            }
        }

        //wait for all active connections to complete before shutting down the server
        await Task.WhenAll(_activeConnections.ToArray());
        _listener.Stop();
        _serverCancellationSource.Dispose();
    }
}
