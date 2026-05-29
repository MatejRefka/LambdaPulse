using LambdaPulse.Engine.Features.Logging;
using LambdaPulse.Engine.Http.Abstractions;
using LambdaPulse.Engine.Shared.Extensions;

namespace LambdaPulse.Engine.Middleware.Implementations;

/// <summary>
/// Sets 404 Not Found response when no response status code has been written.
/// </summary>
internal sealed class TerminationMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "Termination";

    public TerminationMiddleware(Func<WebContext, CancellationToken, Task> nextFunction) : base(nextFunction)
    {
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var downstreamStart = DateTimeOffset.UtcNow;
        var logs = new List<string>();

        if (webContext.WebResponse.StatusCode == null)
        {
            webContext.WebResponse.ClearResponse();

            webContext.WebResponse.StatusCode = 404;
            webContext.WebResponse.ResponsePhrase = "Not Found";
            await webContext.WebResponse.WriteStringToBody("Not Found.", cancellationToken);
            logs.Add("No response status code was set. Set 404 Not Found.");
        }

        RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, logs);
    }
}