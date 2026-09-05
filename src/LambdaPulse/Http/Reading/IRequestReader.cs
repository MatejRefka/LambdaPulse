using System.Net.Sockets;

namespace LambdaPulse.Http.Reading;

/// <summary>
/// Reads raw HTTP requests from a network stream.
/// </summary>
public interface IRequestReader
{
    /// <summary>
    /// Reads the raw HTTP request from the provided network stream asynchronously.
    /// </summary>
    Task<string> ReadHttpRequest(NetworkStream networkStream, CancellationToken cancellationToken = default);
}
