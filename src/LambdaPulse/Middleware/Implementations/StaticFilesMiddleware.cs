using System.Globalization;

using LambdaPulse.Configuration;
using LambdaPulse.Features.Logging;
using LambdaPulse.Http.Abstractions;
using LambdaPulse.Shared.Extensions;

namespace LambdaPulse.Middleware.Implementations;

/// <summary>
/// Serves static files for GET request explicitly targeting a file. 
/// Requests that do not explicitly target a file may be mapped to a static file by the routing middleware. These are served upstream.
/// Static files within /assets/* are set to be aggressively cached via cache busting.
/// Any other static files, including index.html, are set with 'no-cache', forcing the browser to revalidate with the server on each request.
/// </summary>
internal sealed class StaticFilesMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "Static Files";

    private readonly string _fileRootPath;
    private readonly Dictionary<string, string> _mimeTypes;

    public StaticFilesMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, Config config) : base(nextFunction)
    {
        _fileRootPath = Path.GetFullPath(config.ServerConfig.MiddlewareConfig.StaticFilesConfig.FileRootPath);
        //protect against directory traversal
        if (!_fileRootPath.EndsWith(Path.DirectorySeparatorChar))
        {
            _fileRootPath += Path.DirectorySeparatorChar;
        }

        _mimeTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".html", "text/html" },
            { ".css", "text/css" },
            { ".js", "application/javascript" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".svg", "image/svg+xml" },
            { ".ico", "image/x-icon" },
            { ".json", "application/json" },
            { ".txt", "text/plain" },
        };
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var downstreamStart = DateTimeOffset.UtcNow;
        var downstreamLogs = new List<string>();

        if (_fileRootPath.Length == 0)
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "Static file root is not configured. Skip static file handling." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //non-GET requests continue to downstream middleware
        if (!string.Equals(webContext.WebRequest.Method, "GET", StringComparison.OrdinalIgnoreCase))
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { $"{webContext.WebRequest.Method} request. Skip static file handling." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        var requestPath = Uri.UnescapeDataString(webContext.WebRequest.Path);

        //explicit file request
        if (Path.HasExtension(requestPath))
        {
            var relativePathExplicit = requestPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var filePathExplicit = Path.Combine(_fileRootPath, relativePathExplicit);

            await ServeStaticFile(filePathExplicit, requestPath, webContext, downstreamLogs, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.ShortCircuit, downstreamStart, downstreamLogs);
            return;
        }

        //implicit file request. routing middleware will map to a static file
        RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "Request does not explicitly target a file. Defer implicit static file handling until upstream." });
        await _nextFunction(webContext, cancellationToken);

        var upstreamStart = DateTimeOffset.UtcNow;
        var upstreamLogs = new List<string>();

        //no static file mapped
        if (string.IsNullOrWhiteSpace(webContext.StaticFileRelativePath))
        {
            webContext.StaticFileRelativePath = null;
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "No static file mapping. Skip implicit static file handling." });
            return;
        }

        var relativePathImplicit = webContext.StaticFileRelativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var filePathImplicit = Path.Combine(_fileRootPath, relativePathImplicit);
        await ServeStaticFile(filePathImplicit, webContext.StaticFileRelativePath, webContext, upstreamLogs, cancellationToken);

        //cleanup
        webContext.StaticFileRelativePath = null;

        RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, upstreamLogs);
    }

    private async Task ServeStaticFile(string filePath, string relativePath, WebContext webContext, List<string> logs, CancellationToken cancellationToken = default)
    {
        //normalize path
        filePath = Path.GetFullPath(filePath);

        //protect against directory traversal
        if (!filePath.StartsWith(_fileRootPath, StringComparison.OrdinalIgnoreCase))
        {
            webContext.WebResponse.ClearResponse();
            webContext.WebResponse.StatusCode = 400;
            webContext.WebResponse.ResponsePhrase = "Bad Request";
            await webContext.WebResponse.WriteStringToBody("Bad Request.", cancellationToken);
            logs.Add("Static file path escapes the file root.");
            return;
        }

        if (!File.Exists(filePath))
        {
            webContext.WebResponse.ClearResponse();
            webContext.WebResponse.StatusCode = 404;
            webContext.WebResponse.ResponsePhrase = "Not Found";
            await webContext.WebResponse.WriteStringToBody("Not Found.", cancellationToken);
            logs.Add($"Static file was not found. Return 404. path={webContext.WebRequest.Path}.");
            return;
        }

        var extension = Path.GetExtension(filePath);

        //default to application/octet-stream if mime type not found
        if (!_mimeTypes.TryGetValue(extension, out var contentType))
        {
            contentType = "application/octet-stream";
            logs.Add($"File extension is not mapped. Use default content type. extension={extension}.");
        }

        webContext.WebResponse.ClearResponse();

        //aggressive cache for hashed static files
        if (relativePath.StartsWith("/assets/", StringComparison.OrdinalIgnoreCase))
        {
            webContext.WebResponse.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
            logs.Add("Set aggressive cache for hashed static files. Cache-Control=public, max-age=31536000, immutable.");
        }
        //no-cache for other static files, forcing revalidation
        else
        {
            var fileInfo = new FileInfo(filePath);
            var lastModified = fileInfo.LastWriteTimeUtc;
            //round down to nearest second
            lastModified = new DateTime(lastModified.Ticks - (lastModified.Ticks % TimeSpan.TicksPerSecond), DateTimeKind.Utc);
            var eTag = $"W/\"{lastModified.Ticks}-{fileInfo.Length}\"";

            webContext.WebResponse.Headers["Cache-Control"] = "no-cache";
            webContext.WebResponse.Headers["ETag"] = eTag;
            webContext.WebResponse.Headers["Last-Modified"] = lastModified.ToString("R", CultureInfo.InvariantCulture);

            //'If-None-Match' exists
            if (webContext.WebRequest.Headers.TryGetValue("If-None-Match", out var ifNoneMatch))
            {
                var entries = ifNoneMatch.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                //ETag matches, return 304 Not Modified
                if (entries.Any(entry => entry == "*" || string.Equals(entry, eTag, StringComparison.Ordinal)))
                {
                    webContext.WebResponse.StatusCode = 304;
                    webContext.WebResponse.ResponsePhrase = "Not Modified";

                    logs.Add($"ETag matched. Return 304. path={relativePath}.");
                    return;
                }
            }
            //fallback 'If-Modified-Since' exists
            else if (webContext.WebRequest.Headers.TryGetValue("If-Modified-Since", out var ifModifiedSince))
            {
                if (DateTimeOffset.TryParse(ifModifiedSince, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var ifModifiedSinceDate))
                {
                    //file last modified is earlier than or equal to 'If-Modified-Since', return 304 Not Modified
                    if (lastModified <= ifModifiedSinceDate.UtcDateTime)
                    {
                        webContext.WebResponse.StatusCode = 304;
                        webContext.WebResponse.ResponsePhrase = "Not Modified";

                        logs.Add($"File has not changed since If-Modified-Since. Return 304. ifModifiedSince={ifModifiedSince}.");
                        return;
                    }
                }
            }
        }

        var fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);

        webContext.WebResponse.StatusCode = 200;
        webContext.WebResponse.ResponsePhrase = "OK";
        webContext.WebResponse.Headers["Content-Type"] = contentType;

        await webContext.WebResponse.WriteBytesToBody(fileBytes, cancellationToken);
        logs.Add($"Serve static file. path={webContext.WebRequest.Path}.");
    }
}
