using System.Net.Sockets;

namespace LambdaPulse.Server.Services;

public interface IConnectionListener
{
    void Start(int backlog);
    void Stop();
    Task<TcpClient> AcceptTcpClientAsync(CancellationToken cancellationToken);
}
