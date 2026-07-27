using System.Net.Sockets;

namespace LambdaPulse.Http.Reading;

public interface IRequestReader
{
    Task<string> ReadHttpRequest(NetworkStream networkStream, CancellationToken cancellationToken = default);
}
