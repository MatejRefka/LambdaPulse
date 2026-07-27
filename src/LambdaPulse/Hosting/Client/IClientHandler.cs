using System.Net.Sockets;

namespace LambdaPulse.Hosting.Client;

public interface IClientHandler
{
    Task HandleClient(TcpClient tcpClient, CancellationToken serverCancellationToken = default);

}
