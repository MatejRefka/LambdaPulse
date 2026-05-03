using System.Net.Sockets;

namespace LambdaPulse.Engine.Hosting.Connection;

public interface IConnectionListener
{
    void Start(int backlog);
    void Stop();
    Task<TcpClient> AcceptTcpClientAsync(CancellationToken cancellationToken);
}
