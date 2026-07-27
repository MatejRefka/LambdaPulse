using LambdaPulse.Features.Logging;
using LambdaPulse.Http.Abstractions;

namespace LambdaPulse.Middleware;

internal abstract class MiddlewareBase
{
    //delegate pointing to the next function in the pipeline chain
    protected readonly Func<WebContext, CancellationToken, Task> _nextFunction;

    //middleware name used for logging traces
    protected abstract string MiddlewareName { get; }

    public MiddlewareBase(Func<WebContext, CancellationToken, Task> nextFunction)
    {
        _nextFunction = nextFunction;
    }

    //custom logic of the derived middleware
    public abstract Task Invoke(WebContext webContext, CancellationToken cancellationToken = default);

    protected void RecordTelemetry(WebContext webContext, FlowDirection direction, ExecutionEvent executionEvent, DateTimeOffset startTimestamp, List<string>? logs = null)
    {
        webContext.Trace.Steps.Add(new MiddlewareStep
        {
            Middleware = MiddlewareName,
            Direction = direction,
            Event = executionEvent,
            TimestampStart = startTimestamp,
            DurationMs = (float)(DateTimeOffset.UtcNow - startTimestamp).TotalMilliseconds,
            Logs = logs
        });
    }
}
