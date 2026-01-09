using System.Net.Sockets;

namespace LambdaPulse.Server.Hosting.Connection;

public interface IConnectionListener
{
    void Start(int backlog);
    void Stop();
    Task<TcpClient> AcceptTcpClientAsync(CancellationToken cancellationToken);
}
