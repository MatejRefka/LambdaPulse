using LambdaPulse.Engine.Http.Abstractions;
using System.Net.Sockets;

namespace LambdaPulse.Engine.Http.Writing;

public interface IResponseWriter
{
    Task WriteHttpResponse(NetworkStream networkStream, WebContext webContext, CancellationToken cancellationToken);

    Task WriterRaw400Response(NetworkStream networkStream, CancellationToken cancellationToken);
}
