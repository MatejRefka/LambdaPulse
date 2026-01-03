using System.Net.Sockets;

namespace LambdaPulse.Server.Services;

public interface IClientHandler
{
    public Task HandleClient(TcpClient tcpClient, CancellationToken serverCancellationToken);

}
