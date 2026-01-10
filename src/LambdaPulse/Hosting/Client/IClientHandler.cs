using System.Net.Sockets;

namespace LambdaPulse.Server.Hosting.Client;

public interface IClientHandler
{
    Task HandleClient(TcpClient tcpClient, CancellationToken serverCancellationToken);

}
