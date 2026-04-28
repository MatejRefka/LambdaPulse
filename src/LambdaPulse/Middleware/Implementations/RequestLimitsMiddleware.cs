using LambdaPulse.Server.Configuration;
using LambdaPulse.Server.Http.Abstractions;
using LambdaPulse.Server.Shared.Extensions;
using System.Text;

namespace LambdaPulse.Server.Middleware.Implementations;

/// <summary>
/// Enforces request limits, protecting the server from requests that are too large or too slow.
/// Constructs a cancelation source, passing a cancelation token to downstream middleware.
/// </summary>
internal sealed class RequestLimitsMiddleware : MiddlewareBase
{
    private readonly int _maxControlDataSizeBytes;
    private readonly int _maxHeaderSizeBytes;
    private readonly int _maxBodySizeBytes;
    private readonly int _requestReadTimeoutMS;

    public RequestLimitsMiddleware(Func<WebContext, CancellationToken, Task> nextFunction, IConfigProvider configProvider) : base(nextFunction)
    {
        _nextFunction = nextFunction;
        _maxControlDataSizeBytes = configProvider.ServerConfig.MiddlewareConfig.RequestLimitsMiddleware.MaxControlDataSizeBytes;
        _maxHeaderSizeBytes = configProvider.ServerConfig.MiddlewareConfig.RequestLimitsMiddleware.MaxHeaderSizeBytes;
        _maxBodySizeBytes = configProvider.ServerConfig.MiddlewareConfig.RequestLimitsMiddleware.MaxBodySizeBytes;
        _requestReadTimeoutMS = configProvider.ServerConfig.MiddlewareConfig.RequestLimitsMiddleware.RequestReadTimeoutMS;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken)
    {
        //control data size limit
        var controlDataSize = Encoding.UTF8.GetByteCount(webContext.WebRequest.Protocol) + Encoding.UTF8.GetByteCount(webContext.WebRequest.Method) + Encoding.UTF8.GetByteCount(webContext.WebRequest.Path);
        if (controlDataSize > _maxControlDataSizeBytes)
        {
            webContext.WebResponse.StatusCode = 414;
            webContext.WebResponse.ResponsePhrase = "Request URI too long";
            await webContext.WebResponse.WriteStringToBody("Request control data is too large.", cancellationToken);
            return;
        }

        //header size limit
        var headerBytes = webContext.WebRequest.Headers.Sum(header => Encoding.UTF8.GetByteCount(header.Key) + Encoding.UTF8.GetByteCount(header.Value));
        if (headerBytes > _maxHeaderSizeBytes)
        {
            webContext.WebResponse.StatusCode = 431;
            webContext.WebResponse.ResponsePhrase = "Request header fields are too large";
            await webContext.WebResponse.WriteStringToBody("Request header fields are too large.", cancellationToken);
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
                return;
            }
        }

        //sets timeout for long reads (client never finishes sending the request)
        using var requestLimitsCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        requestLimitsCTS.CancelAfter(_requestReadTimeoutMS);

        try
        {
            await _nextFunction(webContext, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[RequestLimits] request timed out");

            //timeout occurred due to client-side or network delay
            if (!webContext.WebResponse.StatusCode.HasValue && !webContext.WebResponse.HasStarted)
            {
                webContext.WebResponse.StatusCode = 408;
                webContext.WebResponse.ResponsePhrase = "Request timed out";
                await webContext.WebResponse.WriteStringToBody("The server did not receive a complete request in time.", cancellationToken);
            }
        }
    }
}
