using LambdaPulse.Engine.Configuration;
using LambdaPulse.Engine.Features.Logging;
using LambdaPulse.Engine.Http.Abstractions;
using LambdaPulse.Engine.Shared.Extensions;

namespace LambdaPulse.Engine.Middleware.Implementations;

/// <summary>
/// Sets the connection response header.
/// Enforces request execution timeout set by the server's configuration via a request-level CTS which is passed to downstream middleware.
/// </summary>
internal sealed class ConnectionMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "Connection";

    private readonly int _requestExecutionTimeoutMS;
    public ConnectionMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, IConfigProvider configProvider) : base(nextFunction)
    {
        _requestExecutionTimeoutMS = configProvider.ServerConfig.MiddlewareConfig.ConnectionMiddleware.RequestExecutionTimeoutMS;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var downstreamStart = DateTimeOffset.UtcNow;

        //request-level token. sets timeout for long mw execution + response write
        using var requestTimeoutCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        requestTimeoutCTS.CancelAfter(_requestExecutionTimeoutMS);

        try
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { $"Time to fully process request: {_requestExecutionTimeoutMS}ms." });
            await _nextFunction(webContext, requestTimeoutCTS.Token);

            var upstreamStart = DateTimeOffset.UtcNow;

            //web server behind reverse proxy -do not interfere
            if (webContext.WebRequest.Headers.ContainsKey("X-Forwarded-For"))
            {
                RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { $"Web server is behind a proxy." });
                return;
            }

            //downstream middleware already set the connection header
            if (webContext.WebResponse.Headers.ContainsKey("Connection"))
            {
                RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { $"Connection header already set by downstream middleware." });
                return;
            }

            //Connection close requested by the client
            if (webContext.WebRequest.Headers.TryGetValue("Connection", out var connectionHeaderValue))
            {
                if (connectionHeaderValue.Equals("close", StringComparison.OrdinalIgnoreCase))
                {
                    webContext.WebResponse.Headers["Connection"] = "close";
                    webContext.ConnectionCloseRequested = true;
                    RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { $"Connection 'close' requested by the client." });
                }
                else
                {
                    webContext.WebResponse.Headers["Connection"] = "keep-alive";
                    RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { $"Connection 'keep-alive' requested by the client." });
                }
            }
            else
            {
                //default to keep-alive if no header specified in request or set by downstream middleware
                webContext.WebResponse.Headers["Connection"] = "keep-alive";
                RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { $"Connection 'keep-alive' set by default." });
            }
        }
        //only catch request execution timeout
        catch (OperationCanceledException) when (requestTimeoutCTS.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            var upstreamStart = DateTimeOffset.UtcNow;

            webContext.WebResponse.StatusCode = 408;
            webContext.WebResponse.ResponsePhrase = "Request timed out";

            webContext.WebResponse.Headers.Clear();
            webContext.WebResponse.Cookies.Clear();

            //close the connection for safety
            webContext.WebResponse.Headers["Connection"] = "close";
            webContext.ConnectionCloseRequested = true;

            webContext.WebResponse.Body.SetLength(0);
            webContext.WebResponse.Body.Position = 0;
            await webContext.WebResponse.WriteStringToBody("The server did not process the request in a timely manner.", cancellationToken);

            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { $"The server did not process the request within {_requestExecutionTimeoutMS}ms.", "Connection 'close' set." });
        }
    }
}
