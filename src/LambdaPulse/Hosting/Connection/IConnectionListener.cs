using System.Net.Sockets;

namespace LambdaPulse.Hosting.Connection;

/// <summary>
/// Connection listener that accepts incoming TCP connections.
/// </summary>
public interface IConnectionListener
{
    /// <summary>
    /// Start listening for incoming TCP connections.
    /// </summary>
    void Start(int backlog);

    /// <summary>
    /// Stop listening for incoming TCP connections.
    /// </summary>
    void Stop();

    /// <summary>
    /// Accept an incoming TCP connection asynchronously.
    /// </summary>
    Task<TcpClient> AcceptTcpClientAsync(CancellationToken cancellationToken = default);
}
