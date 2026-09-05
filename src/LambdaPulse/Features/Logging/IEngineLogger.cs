namespace LambdaPulse.Features.Logging;

/// <summary>
/// Lambda Pulse engine diagnostics logging. Logs the server lifecycle.
/// E.g. socket errors, connection drops
/// </summary>
public interface IEngineLogger
{
    /// <summary>
    /// Logs a message to the engine logger.
    /// </summary>
    void Log(LogLevel logLevel, string source, string message, Exception? exception = null);
}

/// <summary>
/// Defines the severity level of a log message.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Verbose diagnostics.
    /// </summary>
    Debug,

    /// <summary>
    /// Normal operation.
    /// </summary>
    Info,

    /// <summary>
    /// Unexpected behaviour that does not prevent the application from functioning.
    /// </summary>
    Warning,

    /// <summary>
    /// Unexpected behaviour that prevents the application from functioning.
    /// </summary>
    Error
}
