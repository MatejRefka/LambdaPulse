using System.Net.Sockets;

namespace LambdaPulse.Services.Http;

public interface IRequestReader
{
    public Task<string> ReadHttpRequest(NetworkStream networkStream);
}
