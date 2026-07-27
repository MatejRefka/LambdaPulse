using System.Net.Sockets;

namespace LambdaPulse.Hosting.Connection;

public interface IConnectionListener
{
    void Start(int backlog);
    void Stop();
    Task<TcpClient> AcceptTcpClientAsync(CancellationToken cancellationToken = default);
}
