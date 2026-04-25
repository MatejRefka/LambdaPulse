namespace LambdaPulse.Server.Features.Logging;

/// <summary>
/// WebRequest Trace logging. Logs the request lifecycle.
/// E.g. middleware decisions, short-circuits
/// </summary>
public interface ITraceLogger
{
    void Log(Trace trace);
}
