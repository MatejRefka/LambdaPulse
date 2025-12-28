using System.Net.Sockets;

namespace LambdaPulse.Services;

public interface IListener
{
    void Start(int backlog);
    void Stop();
    Task<TcpClient> AcceptTcpClientAsync(CancellationToken cancellationToken);
}
