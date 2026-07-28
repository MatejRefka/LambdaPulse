using LambdaPulse.Configuration;
using LambdaPulse.Features.Logging;
using LambdaPulse.Http.Abstractions;
using LambdaPulse.Shared.Extensions;

namespace LambdaPulse.Middleware.Implementations;

/// <summary>
/// Middleware that blocks incoming HTTP requests from IP addresses declared in config blocklist.
/// </summary>
internal sealed class IpBlocklistMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "IP Blocklist";

    private readonly HashSet<string> _blockedIpAddresses;

    public IpBlocklistMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, Config config) : base(nextFunction)
    {
        _blockedIpAddresses = config.ServerConfig.MiddlewareConfig.IpBlocklistConfig.BlockedIpAddresses;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var downstreamStart = DateTimeOffset.UtcNow;

        if (webContext.RemoteIpAddress != null && _blockedIpAddresses.Contains(webContext.RemoteIpAddress))
        {
            webContext.WebResponse.StatusCode = 403;
            webContext.WebResponse.ResponsePhrase = "Forbidden";
            await webContext.WebResponse.WriteStringToBody("Forbidden.", cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.ShortCircuit, downstreamStart, new List<string> { $"Remote IP is blocklisted. ip={webContext.RemoteIpAddress}." });
            return;
        }

        RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart);
        await _nextFunction(webContext, cancellationToken);
        RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
    }
}
