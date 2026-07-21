using LambdaPulse.Engine.Features.Compression;
using LambdaPulse.Engine.Features.Logging;
using LambdaPulse.Engine.Http.Abstractions;
using LambdaPulse.Engine.Shared.Extensions;

namespace LambdaPulse.Engine.Middleware.Implementations;

/// <summary>
/// Server decides to compresses or to not compress the Body payload of the response. 
/// Decision is based on client-supported encodings, payload size, and MIME type.
/// Adds Content-Encoding header and Vary header for caching.
/// </summary>
internal sealed class ResponseCompressionMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "Response Compression";

    private readonly ICompressor _compressor;
    private readonly List<string> _supportedMimeTypes;
    private const int _minBodySize = 1024;

    public ResponseCompressionMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, ICompressor compressor) : base(nextFunction)
    {
        _compressor = compressor;
        _supportedMimeTypes = new List<string>
        {
            "text/html",
            "text/plain",
            "text/css",
            "application/json",
            "application/javascript",
            "text/javascript",
            "image/svg+xml"
        };
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);

        await _nextFunction(webContext, cancellationToken);

        var upstreamStart = DateTimeOffset.UtcNow;
        var logs = new List<string>();

        //add Accept-Encoding to Vary header for external caching if not already set
        webContext.WebResponse.ApplyVaryHeader("Accept-Encoding");
        logs.Add("Append Accept-Encoding to Vary header for external cache.");

        //body is empty so nothing to compress
        if (!webContext.WebResponse.HasBody)
        {
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Response body is empty. Skip compression." });
            return;
        }

        //body content is already compressed
        if (webContext.WebResponse.Headers.ContainsKey("Content-Encoding"))
        {
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Response is already encoded. Skip compression." });
            return;
        }

        //browser did not advertise any encodings
        webContext.WebRequest.Headers.TryGetValue("Accept-Encoding", out var acceptEncodingValue);
        if (string.IsNullOrWhiteSpace(acceptEncodingValue))
        {
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Accept-Encoding header is missing. Skip compression." });
            return;
        }

        //E.g. Accept-Encoding: gzip, deflate, br
        var encodings = acceptEncodingValue.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        //browser advertised encodings do not match the compressor
        if (!encodings.Any(encoding => string.Equals(encoding, _compressor.Encoding, StringComparison.OrdinalIgnoreCase)))
        {
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Client does not accept the configured encoding. Skip compression." });
            return;
        }

        //no need to compress small payloads
        if (webContext.WebResponse.Body.Length < _minBodySize)
        {
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { $"Response body is below minimum size. Skip compression. minBytes={_minBodySize}." });
            return;
        }

        //MIME type is not set
        webContext.WebResponse.Headers.TryGetValue("Content-Type", out var contentTypeValue);
        if (string.IsNullOrWhiteSpace(contentTypeValue))
        {
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "Content-Type header is missing. Skip compression." });
            return;
        }

        //MIME type is not supported for compression
        if (!_supportedMimeTypes.Any(supportedMimeType => contentTypeValue.Contains(supportedMimeType, StringComparison.OrdinalIgnoreCase)))
        {
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { $"Content-Type is not compressible. Skip compression. contentType={contentTypeValue}." });
            return;
        }

        var uncompressedBody = webContext.WebResponse.Body.ToArray();
        var compressedBody = _compressor.Compress(uncompressedBody);

        //replace body with compressed version
        webContext.WebResponse.Body.SetLength(0);
        webContext.WebResponse.Body.Position = 0;
        await webContext.WebResponse.WriteBytesToBody(compressedBody, cancellationToken);
        webContext.WebResponse.Body.Position = 0;
        logs.Add($"Compress response. encoding={_compressor.Encoding}.");

        webContext.WebResponse.Headers["Content-Encoding"] = _compressor.Encoding;
        logs.Add($"Set response header. Content-Encoding={_compressor.Encoding}.");

        //Content-Length differs so remove header. Response writer will recalculate.
        webContext.WebResponse.Headers.Remove("Content-Length");
        logs.Add("Content-Length must be recalculated. Remove response header.");

        RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, logs);
    }
}
