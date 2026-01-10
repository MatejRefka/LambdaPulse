namespace LambdaPulse.Server.Hosting;

public interface IWebServer : IDisposable
{
    Task StartServer();
    Task StopServer();
}
