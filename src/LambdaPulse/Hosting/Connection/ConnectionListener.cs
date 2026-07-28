using LambdaPulse.Configuration;
using System.Net;
using System.Net.Sockets;

namespace LambdaPulse.Hosting.Connection;

internal sealed class ConnectionListener : IConnectionListener, IDisposable
{
    private readonly TcpListener _listener;

    public ConnectionListener(Config config)
    {
        var address = IPAddress.Parse(config.ServerConfig.Address);
        var port = config.ServerConfig.Port;
        _listener = new TcpListener(address, port);
    }

    public void Start(int backlog)
    {
        _listener.Start(backlog);
    }

    public void Stop()
    {
        _listener.Stop();
    }

    public async Task<TcpClient> AcceptTcpClientAsync(CancellationToken cancellationToken = default)
    {
        return await _listener.AcceptTcpClientAsync(cancellationToken);
    }

    public void Dispose()
    {
        _listener.Dispose();
        GC.SuppressFinalize(this);
    }
}
