using System.Net.Sockets;

namespace LambdaPulse.Engine.Hosting.Client;

public interface IClientHandler
{
    Task HandleClient(TcpClient tcpClient, CancellationToken serverCancellationToken);

}
