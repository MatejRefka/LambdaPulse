namespace LambdaPulse.Server.Features.Logging;

/// <summary>
/// Lambda Pulse engine diagnostics logging. Logs the server lifecycle.
/// E.g. socket errors, connection drops
/// </summary>
public interface IEngineLogger
{
    void Log(EngineLogLevel logLevel, string message, Exception? exception = null);
}

public enum EngineLogLevel { Debug, Info, Warning, Error }
