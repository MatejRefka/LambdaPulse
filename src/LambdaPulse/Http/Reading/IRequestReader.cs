using System.Net.Sockets;

namespace LambdaPulse.Server.Http.Reading;

public interface IRequestReader
{
    public Task<string> ReadHttpRequest(NetworkStream networkStream, CancellationToken cancellationToken);
}
