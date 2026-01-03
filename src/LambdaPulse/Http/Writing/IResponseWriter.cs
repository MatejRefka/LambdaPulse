using LambdaPulse.Server.Services.Http.Models;
using System.Net.Sockets;

namespace LambdaPulse.Server.Services.Http;

public interface IResponseWriter
{
    public Task WriteHttpResponse(NetworkStream networkStream, WebContext webContext);
}
