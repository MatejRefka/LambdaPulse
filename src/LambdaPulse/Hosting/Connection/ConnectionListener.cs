using LambdaPulse.Configuration;
using System.Net;
using System.Net.Sockets;

namespace LambdaPulse.Services;

public class ConnectionListener : IConnectionListener
{
    private readonly System.Net.Sockets.TcpListener _listener;

    public ConnectionListener(IConfigProvider configProvider)
    {
        var address = IPAddress.Parse(configProvider.ServerConfig.Address);
        var port = configProvider.ServerConfig.Port;
        _listener = new System.Net.Sockets.TcpListener(address, port);
    }

    public void Start(int backlog)
    {
        _listener.Start(backlog);
    }

    public void Stop()
    {
        _listener.Stop();
    }

    public async Task<TcpClient> AcceptTcpClientAsync(CancellationToken cancellationToken)
    {
        return await _listener.AcceptTcpClientAsync(cancellationToken);
    }
}
