namespace LambdaPulse.Hosting;

public interface IWebServer : IDisposable
{
    Task StartServer();
    Task StopServer();
}
