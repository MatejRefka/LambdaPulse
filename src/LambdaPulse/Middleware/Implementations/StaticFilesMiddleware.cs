using LambdaPulse.Engine.Configuration;
using LambdaPulse.Engine.Features.Logging;
using LambdaPulse.Engine.Http.Abstractions;
using LambdaPulse.Engine.Shared.Extensions;

namespace LambdaPulse.Engine.Middleware.Implementations;

/// <summary>
/// Serves static files for GET request explicitly targeting a file. 
/// Requests that do not explicitly target a file may be mapped to a static file by the routing middleware. These are served upstream.
/// Non-GET requests are skipped and continue downstream.
/// </summary>
internal sealed class StaticFilesMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "Static Files";

    private readonly string _fileRootPath;
    private readonly Dictionary<string, string> _mimeTypes;

    public StaticFilesMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, IConfigProvider configProvider) : base(nextFunction)
    {
        _fileRootPath = Path.GetFullPath(configProvider.ServerConfig.MiddlewareConfig.StaticFilesMiddleware.FileRootPath);
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

        //non-GET requests continue to downstream middleware
        if (!string.Equals(webContext.WebRequest.Method, "GET", StringComparison.OrdinalIgnoreCase))
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { $"{webContext.WebRequest.Method} request skipped static file handling." });
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

            await ServeStaticFile(filePathExplicit, webContext, downstreamLogs, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.ShortCircuit, downstreamStart, downstreamLogs);
            return;
        }

        //implicit file request. routing middleware will map to a static file
        await _nextFunction(webContext, cancellationToken);

        var upstreamStart = DateTimeOffset.UtcNow;
        var upstreamLogs = new List<string>();

        //no static file mapped
        if (string.IsNullOrWhiteSpace(webContext.StaticFileRelativePath))
        {
            webContext.StaticFileRelativePath = null;
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, new List<string> { "No static file mapping." });
            return;
        }

        var relativePathImplicit = webContext.StaticFileRelativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var filePathImplicit = Path.Combine(_fileRootPath, relativePathImplicit);
        await ServeStaticFile(filePathImplicit, webContext, upstreamLogs, cancellationToken);

        //cleanup
        webContext.StaticFileRelativePath = null;

        RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, upstreamStart, upstreamLogs);
    }

    private async Task ServeStaticFile(string filePath, WebContext webContext, List<string> logs, CancellationToken cancellationToken = default)
    {
        //normalize path
        filePath = Path.GetFullPath(filePath);

        //protect against directory traversal
        if (!filePath.StartsWith(_fileRootPath, StringComparison.OrdinalIgnoreCase))
        {
            webContext.WebResponse.StatusCode = 400;
            webContext.WebResponse.ResponsePhrase = "Bad Request";
            await webContext.WebResponse.WriteStringToBody("Bad Request.", cancellationToken);
            logs.Add("Directory traversal attempt detected, request blocked.");
            return;
        }

        if (!File.Exists(filePath))
        {
            webContext.WebResponse.StatusCode = 404;
            webContext.WebResponse.ResponsePhrase = "Not Found";
            await webContext.WebResponse.WriteStringToBody("Not Found.", cancellationToken);
            logs.Add($"Static file not found: '{webContext.WebRequest.Path}'.");
            return;
        }

        var extension = Path.GetExtension(filePath);

        //default to application/octet-stream if mime type not found
        if (!_mimeTypes.TryGetValue(extension, out var contentType))
        {
            contentType = "application/octet-stream";
            logs.Add($"MIME type for '{extension}' not found, default to application/octet-stream.");
        }

        var fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);

        webContext.WebResponse.StatusCode = 200;
        webContext.WebResponse.ResponsePhrase = "OK";
        webContext.WebResponse.Headers["Content-Type"] = contentType;

        await webContext.WebResponse.WriteBytesToBody(fileBytes, cancellationToken);
        logs.Add($"Static file served: '{webContext.WebRequest.Path}'.");
    }
}
