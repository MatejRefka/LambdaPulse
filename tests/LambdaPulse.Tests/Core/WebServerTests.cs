using LambdaPulse.Configuration;
using LambdaPulse.Hosting;
using System.Net.Sockets;

namespace LambdaPulse.Tests.Core;

public class WebServerTests
{
    [Fact]
    public async Task StartAndStopGracefully()
    {
        //arrange
        var listener = new MockTcpListener();
        var handler = new MockClientHandler();
        var config = new Config();
        var logger = new MockEngineLogger();

        var webServer = new WebServer(listener, handler, config, logger);

        //act
        var serverListeningTask = webServer.StartServer();

        await Task.Delay(100);

        await webServer.StopServer();
        await serverListeningTask;

        //assert
        //no exceptions thrown
    }

    [Fact]
    public async Task DelegateClientToHandler()
    {
        //arrange
        var listener = new MockTcpListener();
        var handler = new MockClientHandler();
        var config = new Config();
        var logger = new MockEngineLogger();

        var webServer = new WebServer(listener, handler, config, logger);

        //act
        var serverListeningTask = webServer.StartServer();

        listener.ConnectWithClient(new TcpClient());

        await Task.Delay(100);

        //assert
        Assert.Equal(1, handler.ClientConnections);

        await webServer.StopServer();
        await serverListeningTask;
    }

    [Fact]
    public async Task SupportMultipleClientsConcurrently()
    {
        //arrange
        var listener = new MockTcpListener();
        var handler = new MockClientHandler();
        var config = new Config();
        var logger = new MockEngineLogger();

        var webServer = new WebServer(listener, handler, config, logger);

        //act
        var serverListeningTask = webServer.StartServer();

        for (int i = 0; i < 20; i++)
        {
            listener.ConnectWithClient(new TcpClient());
        }

        await Task.Delay(100);

        //assert
        Assert.Equal(20, handler.ClientConnections);

        await webServer.StopServer();
        await serverListeningTask;
    }

    [Fact]
    public async Task SingleConnectionErrorDoesNotHaltServer()
    {
        //arrange
        var listener = new MockTcpListener();
        var handler = new MockClientHandler(throwException: true);
        var config = new Config();
        var logger = new MockEngineLogger();

        var webServer = new WebServer(listener, handler, config, logger);

        var serverListeningTask = webServer.StartServer();

        //act
        listener.ConnectWithClient(new TcpClient());
        listener.ConnectWithClient(new TcpClient());

        await Task.Delay(100);

        //assert
        Assert.Equal(2, handler.ClientConnections);

        await webServer.StopServer();
        await serverListeningTask;
    }

    [Fact]
    public async Task UseConfiguredBacklog()
    {
        //arrange
        const int configuredBacklog = 37;
        var listener = new MockTcpListener();
        var handler = new MockClientHandler();
        var config = new Config
        {
            ServerConfig = new ServerConfig
            {
                BackLog = configuredBacklog
            }
        };
        var logger = new MockEngineLogger();

        var webServer = new WebServer(listener, handler, config, logger);

        //act
        var serverListeningTask = webServer.StartServer();
        await Task.Delay(100);
        await webServer.StopServer();
        await serverListeningTask;

        //assert
        Assert.Equal(configuredBacklog, listener.StartedBacklog);
    }
}
