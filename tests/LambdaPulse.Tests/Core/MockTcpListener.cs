using LambdaPulse.Hosting.Connection;
using System.Net.Sockets;
using System.Threading.Channels;

namespace LambdaPulse.Tests.Core;

public class MockTcpListener : IConnectionListener
{
    private readonly Channel<TcpClient> _connections = Channel.CreateUnbounded<TcpClient>();

    public int? StartedBacklog { get; private set; }

    public void Start(int backlog)
    {
        StartedBacklog = backlog;
    }

    public void Stop() { }

    public Task<TcpClient> AcceptTcpClientAsync(CancellationToken cancellationToken = default)
    {
        return _connections.Reader.ReadAsync(cancellationToken).AsTask();
    }

    public void ConnectWithClient(TcpClient client)
    {
        _connections.Writer.TryWrite(client);
    }
}
