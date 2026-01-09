using System.Net.Sockets;

namespace LambdaPulse.Server.Hosting.Client;

public interface IClientHandler
{
    public Task HandleClient(TcpClient tcpClient, CancellationToken serverCancellationToken);

}
