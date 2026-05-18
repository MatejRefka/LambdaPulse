namespace LambdaPulse.Engine.Features.Logging;

/// <summary>
/// Default logger when no logging service is registered.
/// Logs LambdaPulse engine events to the console.
/// </summary>
internal sealed class ConsoleEngineLogger : IEngineLogger
{
    public void Log(LogLevel logLevel, string source, string message, Exception? exception = null)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] [{logLevel}] [{source}] {message}");

        if (exception != null)
        {
            Console.WriteLine(exception.ToString());
        }
    }
}
