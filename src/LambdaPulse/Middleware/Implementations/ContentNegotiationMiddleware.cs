using LambdaPulse.Server.Http.Abstractions;
using LambdaPulse.Server.Shared.Extensions;

namespace LambdaPulse.Server.Middleware.Implementations;

/// <summary>
/// Browser sends an Accept header with preferred mime types. 
/// Middleware negotiates the best mime type to respond with, based on its supported mime types and the Accept header.
/// The negotiated MIME type is stored. User can access it to determine how to format the response body.
/// </summary>
internal sealed class ContentNegotiationMiddleware : MiddlewareBase
{
    private readonly List<string> _supportedMimeTypes;

    public ContentNegotiationMiddleware(Func<WebContext, CancellationToken, Task> nextFunction) : base(nextFunction)
    {
        _nextFunction = nextFunction;
        _supportedMimeTypes = new List<string>
        {
            "application/json",
            "text/html",
            "text/plain"
        };
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        //default mime type is text/html unless it's an api request
        var defaultMimeType = webContext.WebRequest.Path.StartsWith("/api", StringComparison.OrdinalIgnoreCase) ? "application/json" : "text/html";

        var mimeTypes = webContext.WebRequest.Headers.ParseAcceptHeader();

        //accept header is sent
        if (mimeTypes.Count != 0)
        {
            foreach (var mimeType in mimeTypes)
            {
                var matchedMimeType = _supportedMimeTypes.FirstOrDefault(supportedMimeType => string.Equals(supportedMimeType, mimeType, StringComparison.OrdinalIgnoreCase));

                //match requested mime type to a supported mime type
                if (matchedMimeType != null)
                {
                    webContext.NegotiatedMimeType = matchedMimeType;
                    break;
                }
                //match application/* to application/json
                if (string.Equals(mimeType, "application/*", StringComparison.OrdinalIgnoreCase))
                {
                    webContext.NegotiatedMimeType = "application/json";
                    break;
                }
                //match text/* to text/html
                if (string.Equals(mimeType, "text/*", StringComparison.OrdinalIgnoreCase))
                {
                    webContext.NegotiatedMimeType = "text/html";
                    break;
                }
                //accept any mime type
                if (string.Equals(mimeType, "*/*", StringComparison.OrdinalIgnoreCase))
                {
                    webContext.NegotiatedMimeType = defaultMimeType;
                    break;
                }
            }

            if (mimeTypes.Count != 0 && webContext.NegotiatedMimeType == null)
            {
                webContext.WebResponse.StatusCode = 406;
                webContext.WebResponse.ResponsePhrase = "Not Acceptable";
                await webContext.WebResponse.WriteStringToBody("The requested mime type is not supported by the server.", cancellationToken);
                return;
            }
        }
        else
        {
            //no accept header sent
            webContext.NegotiatedMimeType = defaultMimeType;
        }

        await _nextFunction(webContext, cancellationToken);

        //set content-type header if not already set and body was wrtitten to
        if (webContext.WebResponse.HasStarted && !webContext.WebResponse.Headers.ContainsKey("Content-Type"))
        {
            var contentType = webContext.NegotiatedMimeType ?? defaultMimeType;

            //UTF-8 modern standard for text content, tells browser how to interpret raw bytes into characters
            contentType = contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ? contentType + "; charset=utf-8" : contentType;

            webContext.WebResponse.Headers["Content-Type"] = contentType;
        }
    }
}
