namespace LambdaPulse.Engine.Features.Logging;

/// <summary>
/// WebRequest Trace recording. Records the request lifecycle.
/// E.g. middleware decisions, short-circuits
/// </summary>
public interface ITraceRecorder
{
    void Record(Trace trace);
}
