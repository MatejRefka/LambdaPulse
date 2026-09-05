namespace LambdaPulse.Features.Logging;

/// <summary>
/// WebRequest Trace recording. Records the request lifecycle.
/// E.g. middleware decisions, short-circuits
/// </summary>
public interface ITraceRecorder
{
    /// <summary>
    /// Records the request lifecycle trace.
    /// </summary>
    void Record(Trace trace);
}
