using System.Net.Sockets;

namespace LambdaPulse.Hosting.Client;

/// <summary>
/// Handles each incoming TCP client connection.
/// </summary>
public interface IClientHandler
{
    /// <summary>
    /// Handles each client connection on a separate thread.
    /// </summary>
    Task HandleClient(TcpClient tcpClient, CancellationToken serverCancellationToken = default);

}
