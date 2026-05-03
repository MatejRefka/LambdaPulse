namespace LambdaPulse.Engine.Hosting;

public interface IWebServer : IDisposable
{
    Task StartServer();
    Task StopServer();
}
