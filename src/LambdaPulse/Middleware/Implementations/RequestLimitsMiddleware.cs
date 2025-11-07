using LambdaPulse.Configuration.Models;
using LambdaPulse.Services.Http.Models;
using System.Text;

namespace LambdaPulse.Middleware.Implementations;

/// <summary>
/// Enforces request limits, protecting the server from requests that are too large or too slow.
/// Constructs a cancelation source, passing a cancelation token to downstream middleware.
/// </summary>
public sealed class RequestLimits : MiddlewareBase
{
    private readonly Config _config;
    public RequestLimits(Func<WebContext, CancellationToken, Task> nextFunction, Config config) : base(nextFunction)
    {
        _nextFunction = nextFunction;
        _config = config;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken)
    {
        //control data size limit
        var controlDataSize = Encoding.UTF8.GetByteCount(webContext.WebRequest.Protocol) + Encoding.UTF8.GetByteCount(webContext.WebRequest.Method) + Encoding.UTF8.GetByteCount(webContext.WebRequest.Path);
        if (controlDataSize > _config.ServerConfig.MiddlewareConfig.MaxControlDataSizeBytes)
        {
            webContext.WebResponse.StatusCode = 414;
            webContext.WebResponse.ResponsePhrase = "Request control data is too large.";
            return;
        }

        //header size limit
        var headerBytes = webContext.WebRequest.Headers.Sum(header => Encoding.UTF8.GetByteCount(header.Key) + Encoding.UTF8.GetByteCount(header.Value));
        if (headerBytes > _config.ServerConfig.MiddlewareConfig.MaxHeaderSizeBytes)
        {
            webContext.WebResponse.StatusCode = 431;
            webContext.WebResponse.ResponsePhrase = "Request header fields are too large.";
            return;
        }

        //body size limit
        if (webContext.WebRequest.Body?.Length > 0)
        {
            var bodyBytes = Encoding.UTF8.GetByteCount(webContext.WebRequest.Body);
            if (bodyBytes > _config.ServerConfig.MiddlewareConfig.MaxBodySizeBytes)
            {
                webContext.WebResponse.StatusCode = 413;
                webContext.WebResponse.ResponsePhrase = "Request body is too large.";
                return;
            }
        }

        //sets timeout for long reads (client never finishes sending the request)
        using var requestLimitsCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        requestLimitsCTS.CancelAfter(_config.ServerConfig.MiddlewareConfig.RequestReadTimeoutMS);

        try
        {
            Console.WriteLine($"[RequestLimits] logic performed on WebRequest");
            await _nextFunction(webContext, cancellationToken);
            Console.WriteLine($"[RequestLimits] logic performed on WebResponse");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[RequestLimits] request timed out");

            //timeout occurred due to client-side or network delay
            if (!webContext.WebResponse.StatusCode.HasValue && !webContext.WebResponse.HasStarted)
            {
                webContext.WebResponse.StatusCode = 408;
                webContext.WebResponse.ResponsePhrase = "Request timed out.";
            }

            return;
        }
    }
}
