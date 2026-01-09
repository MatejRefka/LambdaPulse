namespace LambdaPulse.Server.Hosting;

public interface IWebServer : IDisposable
{
    public Task StartServer();
    public Task StopServer();
}
