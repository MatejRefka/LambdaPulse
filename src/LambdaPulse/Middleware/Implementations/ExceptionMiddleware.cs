using LambdaPulse.Features.Logging;
using LambdaPulse.Http.Abstractions;
using LambdaPulse.Shared.Extensions;

namespace LambdaPulse.Middleware.Implementations;

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
            var upstreamStart = DateTimeOffset.UtcNow;
            _engineLogger.Log(LogLevel.Error, "ExceptionMiddleware", "Pipeline threw an unhandled exception.", e);

            webContext.WebResponse.ClearResponse();

            webContext.WebResponse.StatusCode = 500;
            webContext.WebResponse.ResponsePhrase = "Internal Server Error";

            await webContext.WebResponse.WriteStringToBody("The server encountered an unexpected condition that prevented it from fulfilling the request.", cancellationToken);

            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Unhandled exception reached exception middleware.", "Clear response.", "Return 500." });
        }
    }
}
