using LambdaPulse.Server.Hosting.Connection;
using System.Net.Sockets;
using System.Threading.Channels;

namespace LambdaPulse.Tests.Core;

public class MockTcpListener : IConnectionListener
{
    private readonly Channel<TcpClient> _connections = Channel.CreateUnbounded<TcpClient>();

    public void Start(int backlog) { }

    public void Stop() { }

    public Task<TcpClient> AcceptTcpClientAsync(CancellationToken cancellationToken)
    {
        return _connections.Reader.ReadAsync(cancellationToken).AsTask();
    }

    public void ConnectWithClient(TcpClient client)
    {
        _connections.Writer.TryWrite(client);
    }
}
