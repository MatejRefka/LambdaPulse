using System.Net.Sockets;

using LambdaPulse.Http.Abstractions;

namespace LambdaPulse.Http.Writing;

/// <summary>
/// Writes HTTP responses to a network stream.
/// </summary>
public interface IResponseWriter
{
    /// <summary>
    /// Writes a HTTP response to the network stream.
    /// </summary>
    Task WriteHttpResponse(NetworkStream networkStream, WebContext webContext, CancellationToken cancellationToken);

    /// <summary>
    /// Writes a raw HTTP 400 Bad Request response to the network stream.
    /// </summary>
    Task WriterRaw400Response(NetworkStream networkStream, CancellationToken cancellationToken);
}
