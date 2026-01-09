using LambdaPulse.Server.Features.Compression;
using LambdaPulse.Server.Http.Abstractions;
using LambdaPulse.Server.Shared.Extensions;

namespace LambdaPulse.Server.Middleware.Implementations;

/// <summary>
/// Compresses Body payload of the response. Based on client-supported encodings, payload size, and MIME type.
/// Adds Content-Encoding header and Vary header for caching.
/// </summary>
public sealed class ResponseCompressionMiddleware : MiddlewareBase
{
    private readonly ICompressor _compressor;
    private readonly List<string> _supportedMimeTypes;
    private const int _minBodySize = 1024;

    public ResponseCompressionMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, ICompressor compressor) : base(nextFunction)
    {
        _nextFunction = nextFunction;
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
        await _nextFunction(webContext, cancellationToken);

        //body is empty so nothing to compress
        if (!webContext.WebResponse.HasStarted)
        {
            return;
        }

        //body content is already compressed
        if (webContext.WebResponse.Headers.ContainsKey("Content-Encoding"))
        {
            return;
        }

        //browser did not advertise any encodings
        webContext.WebRequest.Headers.TryGetValue("Accept-Encoding", out var acceptEncodingValue);
        if (string.IsNullOrEmpty(acceptEncodingValue))
        {
            return;
        }

        //E.g. Accept-Encoding: gzip, deflate, br
        var encodings = acceptEncodingValue.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        //browser advertised encodings do not match the compressor
        if (!encodings.Any(encoding => string.Equals(encoding, _compressor.Encoding, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        //no need to compress small payloads
        if (webContext.WebResponse.Body.Length < _minBodySize)
        {
            return;
        }

        webContext.WebResponse.Headers.TryGetValue("Content-Type", out var contentTypeValue);
        if (string.IsNullOrEmpty(contentTypeValue))
        {
            return;
        }
        //MIME type is not supported for compression
        if (!_supportedMimeTypes.Any(supportedMimeType => contentTypeValue.Contains(supportedMimeType, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var uncompressedBody = webContext.WebResponse.Body.ToArray();
        var compressedBody = _compressor.Compress(uncompressedBody);

        //replace body with compressed version
        webContext.WebResponse.Body.SetLength(0);
        webContext.WebResponse.Body.Position = 0;
        await webContext.WebResponse.WriteBytesToBody(compressedBody, cancellationToken);
        webContext.WebResponse.Body.Position = 0;

        webContext.WebResponse.Headers["Content-Encoding"] = _compressor.Encoding;

        //Content-Length differs so remove header. Response writer will recalculate.
        webContext.WebResponse.Headers.Remove("Content-Length");

        //add Accept-Encoding to Vary header for caching if not already set
        webContext.WebResponse.Headers.TryGetValue("Vary", out var varyHeaderValue);
        if (string.IsNullOrEmpty(varyHeaderValue))
        {
            webContext.WebResponse.Headers["Vary"] = "Accept-Encoding";
        }
        else if (!varyHeaderValue.Contains("Accept-Encoding"))
        {
            webContext.WebResponse.Headers["Vary"] = $"{varyHeaderValue}, Accept-Encoding";
        }
    }
}
