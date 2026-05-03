namespace LambdaPulse.Engine.Features.Logging;

/// <summary>
/// Lambda Pulse engine diagnostics logging. Logs the server lifecycle.
/// E.g. socket errors, connection drops
/// </summary>
public interface IEngineLogger
{
    void Log(LogLevel logLevel, string message, Exception? exception = null);
}

public enum LogLevel { Debug, Info, Warning, Error }
