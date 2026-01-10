using LambdaPulse.Server.Http.Abstractions;
using System.Net.Sockets;

namespace LambdaPulse.Server.Http.Writing;

public interface IResponseWriter
{
    Task WriteHttpResponse(NetworkStream networkStream, WebContext webContext);
}
