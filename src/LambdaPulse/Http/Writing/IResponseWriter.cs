using LambdaPulse.Server.Http.Abstractions;
using System.Net.Sockets;

namespace LambdaPulse.Server.Http.Writing;

public interface IResponseWriter
{
    public Task WriteHttpResponse(NetworkStream networkStream, WebContext webContext);
}
