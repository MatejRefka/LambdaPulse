namespace LambdaPulse.Engine.Features.Logging;

internal sealed class NullTraceRecorder : ITraceRecorder
{
    public void Record(Trace trace)
    {
        //do not record
    }
}
