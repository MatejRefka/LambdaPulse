using LambdaPulse.Server.Services;
using System.Net.Sockets;

namespace LambdaPulse.Tests.Core;

public class MockClientHandler : IClientHandler
{
    private readonly bool _throwException;
    public int ClientConnections { get; private set; }

    public MockClientHandler(bool throwException = false)
    {
        _throwException = throwException;
    }

    public Task HandleClient(TcpClient tcpClient, CancellationToken serverCancellationToken)
    {
        ClientConnections++;

        if (_throwException)
        {
            throw new InvalidOperationException("ERROR");
        }

        tcpClient.Dispose();
        return Task.CompletedTask;
    }
}
