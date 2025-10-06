using System.Net.Sockets;

namespace LambdaPulse.Services;

public interface IClientHandler
{
    public Task HandleClient(TcpClient tcpClient);

}
