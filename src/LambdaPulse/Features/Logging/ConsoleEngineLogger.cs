namespace LambdaPulse.Server.Features.Logging;

/// <summary>
/// Default logger when no logging service is registered.
/// Logs LambdaPulse engine events to the console.
/// </summary>
internal sealed class ConsoleEngineLogger : IEngineLogger
{
    public void Log(EngineLogLevel logLevel, string message, Exception? exception = null)
    {
        Console.WriteLine($"[{DateTime.UtcNow:O}] [{logLevel}] {message}");

        if (exception != null)
        {
            Console.WriteLine(exception.ToString());
        }
    }
}
