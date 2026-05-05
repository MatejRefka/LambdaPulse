using LambdaPulse.Engine.Configuration;
using LambdaPulse.Engine.Http.Abstractions;
using LambdaPulse.Engine.Shared.Extensions;

namespace LambdaPulse.Engine.Middleware.Implementations;

internal sealed class StaticFilesMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "Static Files";

    private readonly string _fileRootPath;
    private readonly Dictionary<string, string> _mimeTypes;

    public StaticFilesMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, IConfigProvider configProvider) : base(nextFunction)
    {
        _fileRootPath = configProvider.ServerConfig.MiddlewareConfig.StaticFilesMiddleware.FileRootPath;

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
        //non-GET requests continue to downstream middleware
        if (webContext.WebRequest.Method != "GET")
        {
            await _nextFunction(webContext, cancellationToken);
            return;
        }

        var requestPath = webContext.WebRequest.Path;

        //explicit file request
        if (Path.HasExtension(requestPath))
        {
            var relativePathExplicit = requestPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var filePathExplicit = Path.Combine(_fileRootPath, relativePathExplicit);

            await ServeStaticFile(filePathExplicit, webContext, cancellationToken);
            return;
        }

        //implicit file request. routing middleware will map to a static file
        await _nextFunction(webContext, cancellationToken);

        //user has written a response or no static file mapped
        if (webContext.WebResponse.HasStarted || string.IsNullOrWhiteSpace(webContext.StaticFileRelativePath))
        {
            webContext.StaticFileRelativePath = null;
            return;
        }

        var relativePathImplicit = webContext.StaticFileRelativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var filePathImplicit = Path.Combine(_fileRootPath, relativePathImplicit);
        await ServeStaticFile(filePathImplicit, webContext, cancellationToken);

        //cleanup
        webContext.StaticFileRelativePath = null;
    }

    private async Task ServeStaticFile(string filePath, WebContext webContext, CancellationToken cancellationToken)
    {
        //normalize path
        filePath = Path.GetFullPath(filePath);

        //protect against directory traversal
        if (!filePath.StartsWith(_fileRootPath, StringComparison.OrdinalIgnoreCase))
        {
            webContext.WebResponse.StatusCode = 400;
            webContext.WebResponse.ResponsePhrase = "Bad Request";
            await webContext.WebResponse.WriteStringToBody("Bad Request.", cancellationToken);
            return;
        }

        if (!File.Exists(filePath))
        {
            webContext.WebResponse.StatusCode = 404;
            webContext.WebResponse.ResponsePhrase = "Not Found";
            await webContext.WebResponse.WriteStringToBody("Not Found.", cancellationToken);
            return;
        }

        var extension = Path.GetExtension(filePath);

        //default to application/octet-stream if mime type not found
        if (!_mimeTypes.TryGetValue(extension, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        var fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);

        webContext.WebResponse.StatusCode = 200;
        webContext.WebResponse.ResponsePhrase = "OK";
        webContext.WebResponse.Headers["Content-Type"] = contentType;

        await webContext.WebResponse.WriteBytesToBody(fileBytes, cancellationToken);
    }
}
