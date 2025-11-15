using LambdaPulse.Services.Http.Models;
using System.Net.Sockets;

namespace LambdaPulse.Services.Http;

public interface IResponseWriter
{
    public Task WriteHttpResponse(NetworkStream networkStream, WebResponse webResponse);
}
