namespace LambdaPulse.Hosting;

/// <summary>
/// Defines web server operations.
/// </summary>
public interface IWebServer : IDisposable
{
    /// <summary>
    /// Start the web server.
    /// </summary>
    Task StartServer();

    /// <summary>
    /// Stop the web server.
    /// </summary>
    Task StopServer();
}
