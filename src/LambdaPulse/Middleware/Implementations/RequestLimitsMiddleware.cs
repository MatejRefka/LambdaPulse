using LambdaPulse.Engine.Configuration;
using LambdaPulse.Engine.Features.Logging;
using LambdaPulse.Engine.Http.Abstractions;
using LambdaPulse.Engine.Shared.Extensions;
using System.Text;

namespace LambdaPulse.Engine.Middleware.Implementations;

/// <summary>
/// Enforces request limits, protecting the server from requests that are too large.
/// </summary>
internal sealed class RequestLimitsMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "Request Limits";

    private readonly int _maxControlDataSizeBytes;
    private readonly int _maxHeaderSizeBytes;
    private readonly int _maxBodySizeBytes;
    private readonly IEngineLogger _engineLogger;

    public RequestLimitsMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, IConfigProvider configProvider, IEngineLogger engineLogger) : base(nextFunction)
    {
        _maxControlDataSizeBytes = configProvider.ServerConfig.MiddlewareConfig.RequestLimitsMiddleware.MaxControlDataSizeBytes;
        _maxHeaderSizeBytes = configProvider.ServerConfig.MiddlewareConfig.RequestLimitsMiddleware.MaxHeaderSizeBytes;
        _maxBodySizeBytes = configProvider.ServerConfig.MiddlewareConfig.RequestLimitsMiddleware.MaxBodySizeBytes;
        _engineLogger = engineLogger;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var downstreamStart = DateTimeOffset.UtcNow;

        //control data size limit
        var controlDataSize = Encoding.UTF8.GetByteCount(webContext.WebRequest.Protocol) + Encoding.UTF8.GetByteCount(webContext.WebRequest.Method) + Encoding.UTF8.GetByteCount(webContext.WebRequest.Path);
        if (controlDataSize > _maxControlDataSizeBytes)
        {
            webContext.WebResponse.StatusCode = 414;
            webContext.WebResponse.ResponsePhrase = "Request URI too long";
            await webContext.WebResponse.WriteStringToBody("Request control data is too large.", cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.ShortCircuit, downstreamStart, new List<string> { $"Request control data is too large (URL). Maximum bytes allowed: {_maxControlDataSizeBytes}." });
            _engineLogger.Log(LogLevel.Warning, "RequestLimitsMiddleware", $"Request control data is too large. Method: {webContext.WebRequest.Method}, Path: {webContext.WebRequest.Path}, Protocol: {webContext.WebRequest.Protocol}, Size: {controlDataSize}bytes. Maximum allowed: {_maxControlDataSizeBytes}bytes. Remote Ip Address: {webContext.RemoteIpAddress}");
            return;
        }

        //header size limit
        var headerBytes = webContext.WebRequest.Headers.Sum(header => Encoding.UTF8.GetByteCount(header.Key) + Encoding.UTF8.GetByteCount(header.Value));
        if (headerBytes > _maxHeaderSizeBytes)
        {
            webContext.WebResponse.StatusCode = 431;
            webContext.WebResponse.ResponsePhrase = "Request header fields are too large";
            await webContext.WebResponse.WriteStringToBody("Request header fields are too large.", cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.ShortCircuit, downstreamStart, new List<string> { $"Request header fields are too large. Maximum bytes allowed: {_maxHeaderSizeBytes}." });
            _engineLogger.Log(LogLevel.Warning, "RequestLimitsMiddleware", $"Request header fields are too large. Method: {webContext.WebRequest.Method}, Path: {webContext.WebRequest.Path}, Protocol: {webContext.WebRequest.Protocol}, Size: {headerBytes}bytes. Maximum allowed: {_maxHeaderSizeBytes}bytes. Remote Ip Address: {webContext.RemoteIpAddress}");
            return;
        }

        //body size limit
        if (webContext.WebRequest.Body?.Length > 0)
        {
            var bodyBytes = Encoding.UTF8.GetByteCount(webContext.WebRequest.Body);
            if (bodyBytes > _maxBodySizeBytes)
            {
                webContext.WebResponse.StatusCode = 413;
                webContext.WebResponse.ResponsePhrase = "Request body is too large";
                await webContext.WebResponse.WriteStringToBody("Request body is too large.", cancellationToken);
                RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.ShortCircuit, downstreamStart, new List<string> { $"Request body is too large. Maximum bytes allowed: {_maxBodySizeBytes}." });
                _engineLogger.Log(LogLevel.Warning, "RequestLimitsMiddleware", $"Request body is too large. Method: {webContext.WebRequest.Method}, Path: {webContext.WebRequest.Path}, Protocol: {webContext.WebRequest.Protocol}, Size: {bodyBytes}bytes. Maximum allowed: {_maxBodySizeBytes}bytes. Remote Ip Address: {webContext.RemoteIpAddress}");
                return;
            }
        }

        RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart);
        await _nextFunction(webContext, cancellationToken);
        RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
    }
}
