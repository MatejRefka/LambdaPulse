using LambdaPulse.Configuration;
using LambdaPulse.Services.Http.Models;
using LambdaPulse.Utility.Extensions;

namespace LambdaPulse.Middleware.Implementations;

public sealed class StaticFilesMiddleware : MiddlewareBase
{
    private readonly string _fileRootPath;
    private readonly Dictionary<string, string> _mimeTypes;

    public StaticFilesMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, IConfigProvider configProvider) : base(nextFunction)
    {
        _nextFunction = nextFunction;
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
        if (!string.Equals(webContext.WebRequest.Method, "GET", StringComparison.OrdinalIgnoreCase))
        {
            await _nextFunction(webContext, cancellationToken);
            return;
        }

        var requestPath = webContext.WebRequest.Path;

        //only handle explicit file requests
        if (string.IsNullOrEmpty(requestPath) || !Path.HasExtension(requestPath))
        {
            await _nextFunction(webContext, cancellationToken);
            return;
        }

        var relativePath = requestPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);

        var fullFilePath = Path.Combine(_fileRootPath, relativePath);

        //protect against directory traversal
        if (!fullFilePath.StartsWith(_fileRootPath, StringComparison.OrdinalIgnoreCase))
        {
            await _nextFunction(webContext, cancellationToken);
            return;
        }

        //ensure file exists
        if (!File.Exists(fullFilePath))
        {
            await _nextFunction(webContext, cancellationToken);
            webContext.WebResponse.StatusCode = 404;
            webContext.WebResponse.ResponsePhrase = "Not Found";
            await webContext.WebResponse.WriteToBody("Not Found");
            return;
        }

        var extension = Path.GetExtension(fullFilePath);

        //default to application/octet-stream if mime type not found
        if (!_mimeTypes.TryGetValue(extension, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        var fileBytes = await File.ReadAllBytesAsync(fullFilePath, cancellationToken);

        webContext.WebResponse.StatusCode = 200;
        webContext.WebResponse.Headers["Content-Type"] = contentType;

        await webContext.WebResponse.Body.WriteAsync(fileBytes, cancellationToken);
        return;
    }
}
