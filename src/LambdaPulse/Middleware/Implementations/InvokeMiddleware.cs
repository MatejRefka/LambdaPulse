using LambdaPulse.Features.Logging;
using LambdaPulse.Http.Abstractions;

namespace LambdaPulse.Middleware.Implementations;

/// <summary>
/// Executes user code that's mapped to the requested endpoint.
/// </summary>
internal sealed class InvokeMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "Invoke";

    public InvokeMiddleware(Func<WebContext, CancellationToken, Task> nextFunction) : base(nextFunction)
    {
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var downstreamStart = DateTimeOffset.UtcNow;
        var logs = new List<string>();

        if (webContext.Endpoint != null)
        {
            //user application code is invoked here
            await webContext.Endpoint.ApplicationFunction(webContext, cancellationToken);
            logs.Add("Endpoint matched the request. Invoke endpoint.");
        }

        RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart);
        await _nextFunction(webContext, cancellationToken);
        RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
    }
}
