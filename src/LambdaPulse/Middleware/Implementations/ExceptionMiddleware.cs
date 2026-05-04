using LambdaPulse.Engine.Features.Logging;
using LambdaPulse.Engine.Http.Abstractions;
using LambdaPulse.Engine.Shared.Extensions;

namespace LambdaPulse.Engine.Middleware.Implementations;

/// <summary>
/// Wraps the pipeline in a try/catch, ensuring the whole server does not crash.
/// Any unhandled exception type implementing Exception responds with 500 response.
/// </summary>
internal sealed class ExceptionMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "Exception";

    private readonly IEngineLogger _engineLogger;

    public ExceptionMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, IEngineLogger engineLogger) : base(nextFunction)
    {
        _nextFunction = nextFunction;
        _engineLogger = engineLogger;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        try
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
        }
        catch (Exception e)
        {
            var downstreamStart = DateTimeOffset.UtcNow;
            _engineLogger.Log(LogLevel.Error, "ExceptionMiddleware", "Pipeline threw an unhandled exception.", e);
            var telemetryLogs = new List<string>();

            //do not overwrite the response as it could be being written to
            if (!webContext.WebResponse.HasStarted)
            {
                webContext.WebResponse.StatusCode = 500;
                webContext.WebResponse.ResponsePhrase = "Internal Server Error";
                await webContext.WebResponse.WriteStringToBody("The server encountered an unexpected condition that prevented it from fulfilling the request.", cancellationToken);

                //clear response headers
                webContext.WebResponse.Headers = new Dictionary<string, string>()
                {
                    ["Content-Type"] = "text/plain; charset=utf-8"
                };

                telemetryLogs.Add("500 response written. Response headers cleared.");
            }
            else
            {
                telemetryLogs.Add("Exception thrown after the response has started being written.");
            }

            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, downstreamStart, telemetryLogs);
        }
    }
}
