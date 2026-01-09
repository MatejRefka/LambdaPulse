using System.Net.Sockets;

namespace LambdaPulse.Server.Hosting.Clients;

public interface IClientHandler
{
    public Task HandleClient(TcpClient tcpClient, CancellationToken serverCancellationToken);

}
