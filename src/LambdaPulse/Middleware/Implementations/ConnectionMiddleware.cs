using LambdaPulse.Configuration;
using LambdaPulse.Features.Logging;
using LambdaPulse.Http.Abstractions;
using LambdaPulse.Shared.Extensions;

namespace LambdaPulse.Middleware.Implementations;

/// <summary>
/// Sets the connection response header.
/// Enforces request execution timeout set by the server's configuration via a request-level CTS which is passed to downstream middleware.
/// </summary>
internal sealed class ConnectionMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "Connection";

    private readonly int _requestExecutionTimeoutMS;
    public ConnectionMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, Config config) : base(nextFunction)
    {
        _requestExecutionTimeoutMS = config.ServerConfig.MiddlewareConfig.ConnectionConfig.RequestExecutionTimeoutMS;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var downstreamStart = DateTimeOffset.UtcNow;

        //skip request execution timeout for SSE requests (GET accepting text/event-stream)
        if (webContext.WebRequest.Method.Equals("GET", StringComparison.OrdinalIgnoreCase) && webContext.WebRequest.Headers.TryGetValue("Accept", out var acceptHeaderValue) && acceptHeaderValue.Contains("text/event-stream", StringComparison.OrdinalIgnoreCase))
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "Server-sent events request detected. Skip request execution timeout." });
            await _nextFunction(webContext, cancellationToken);

            var upstreamStart = DateTimeOffset.UtcNow;

            //web server behind reverse proxy -do not interfere
            if (webContext.WebRequest.Headers.ContainsKey("X-Forwarded-For"))
            {
                RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Request forwarded by a proxy." });
                return;
            }

            //downstream middleware already set the connection header
            if (webContext.WebResponse.Headers.ContainsKey("Connection"))
            {
                RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Connection header already set by downstream middleware." });
                return;
            }

            //Connection close requested by the client
            if (webContext.WebRequest.Headers.TryGetValue("Connection", out var connectionHeaderValue))
            {
                if (connectionHeaderValue.Equals("close", StringComparison.OrdinalIgnoreCase))
                {
                    webContext.WebResponse.Headers["Connection"] = "close";
                    webContext.ConnectionCloseRequested = true;
                    RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Client requested Connection: close. Close connection." });
                }
                else
                {
                    webContext.WebResponse.Headers["Connection"] = "keep-alive";
                    RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Client did not request Connection: close. Keep connection alive." });
                }
            }
            else
            {
                //default to keep-alive if no header specified in request or set by downstream middleware
                webContext.WebResponse.Headers["Connection"] = "keep-alive";
                RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Client did not provide a Connection preference. Keep connection alive." });
            }
            return;
        }

        //request-level token. sets timeout for long mw execution + response write
        using var requestTimeoutCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        requestTimeoutCTS.CancelAfter(_requestExecutionTimeoutMS);

        try
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { $"Request timeout configured. Enforce request timeout. timeoutMs={_requestExecutionTimeoutMS}." });
            await _nextFunction(webContext, requestTimeoutCTS.Token);

            var upstreamStart = DateTimeOffset.UtcNow;

            //web server behind reverse proxy -do not interfere
            if (webContext.WebRequest.Headers.ContainsKey("X-Forwarded-For"))
            {
                RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Request forwarded by a proxy." });
                return;
            }

            //downstream middleware already set the connection header
            if (webContext.WebResponse.Headers.ContainsKey("Connection"))
            {
                RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Connection header already set by downstream middleware." });
                return;
            }

            //Connection close requested by the client
            if (webContext.WebRequest.Headers.TryGetValue("Connection", out var connectionHeaderValue))
            {
                if (connectionHeaderValue.Equals("close", StringComparison.OrdinalIgnoreCase))
                {
                    webContext.WebResponse.Headers["Connection"] = "close";
                    webContext.ConnectionCloseRequested = true;
                    RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Client requested Connection: close. Close connection." });
                }
                else
                {
                    webContext.WebResponse.Headers["Connection"] = "keep-alive";
                    RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Client did not request Connection: close. Keep connection alive." });
                }
            }
            else
            {
                //default to keep-alive if no header specified in request or set by downstream middleware
                webContext.WebResponse.Headers["Connection"] = "keep-alive";
                RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Client did not provide a Connection preference. Keep connection alive." });
            }
        }
        //only catch request execution timeout
        catch (OperationCanceledException) when (requestTimeoutCTS.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            var upstreamStart = DateTimeOffset.UtcNow;

            webContext.WebResponse.ClearResponse();

            webContext.WebResponse.StatusCode = 408;
            webContext.WebResponse.ResponsePhrase = "Request timed out";

            //close the connection for safety
            webContext.WebResponse.Headers["Connection"] = "close";
            webContext.ConnectionCloseRequested = true;

            await webContext.WebResponse.WriteStringToBody("The server did not process the request in a timely manner.", cancellationToken);

            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { $"Request exceeded timeoutMs={_requestExecutionTimeoutMS}. Return timeout response.", "Request timed out. Close connection." });
        }
    }
}
