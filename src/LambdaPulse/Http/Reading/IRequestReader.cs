using System.Net.Sockets;

namespace LambdaPulse.Server.Services.Http;

public interface IRequestReader
{
    public Task<string> ReadHttpRequest(NetworkStream networkStream, CancellationToken cancellationToken);
}
