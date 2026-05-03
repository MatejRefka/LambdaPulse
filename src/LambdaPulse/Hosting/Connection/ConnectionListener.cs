using LambdaPulse.Engine.Configuration;
using System.Net;
using System.Net.Sockets;

namespace LambdaPulse.Engine.Hosting.Connection;

internal sealed class ConnectionListener : IConnectionListener, IDisposable
{
    private readonly TcpListener _listener;

    public ConnectionListener(IConfigProvider configProvider)
    {
        var address = IPAddress.Parse(configProvider.ServerConfig.Address);
        var port = configProvider.ServerConfig.Port;
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

    public async Task<TcpClient> AcceptTcpClientAsync(CancellationToken cancellationToken)
    {
        return await _listener.AcceptTcpClientAsync(cancellationToken);
    }

    public void Dispose()
    {
        _listener.Dispose();
        GC.SuppressFinalize(this);
    }
}
