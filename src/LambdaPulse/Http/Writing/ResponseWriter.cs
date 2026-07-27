using LambdaPulse.Http.Abstractions;
using System.Globalization;
using System.Net.Sockets;
using System.Text;

namespace LambdaPulse.Http.Writing;

internal sealed class ResponseWriter : IResponseWriter
{
    public async Task WriteHttpResponse(NetworkStream networkStream, WebContext webContext, CancellationToken cancellationToken = default)
    {
        var responseStatusLine = $"HTTP/1.1 {webContext.WebResponse.StatusCode} {webContext.WebResponse.ResponsePhrase}\r\n";

        webContext.WebResponse.Body.Position = 0;
        var bodyBytes = webContext.WebResponse.Body.ToArray();

        //recommended response headers for HTTP/1.1
        if (!webContext.WebResponse.Headers.ContainsKey("Content-Length"))
        {
            webContext.WebResponse.Headers["Content-Length"] = bodyBytes.Length > 0 ? bodyBytes.Length.ToString(CultureInfo.InvariantCulture) : "0";
        }
        if (!webContext.WebResponse.Headers.ContainsKey("Content-Type"))
        {
            webContext.WebResponse.Headers["Content-Type"] = "text/plain; charset=utf-8";
        }
        if (!webContext.WebResponse.Headers.ContainsKey("Connection") && !webContext.WebRequest.Headers.ContainsKey("X-Forwarded-For"))
        {
            webContext.WebResponse.Headers["Connection"] = webContext.WebResponse.Headers["Connection"] = "close";
        }

        //parse WebResponse headers into string
        string headersBlock = responseStatusLine;
        foreach (var header in webContext.WebResponse.Headers)
        {
            headersBlock += $"{header.Key}: {header.Value}\r\n";
        }
        //parse Cookies into the headers block
        foreach (var cookie in webContext.WebResponse.Cookies)
        {
            headersBlock += $"Set-Cookie: {cookie}\r\n";
        }

        headersBlock += "\r\n";

        //encode headers
        var headerBytes = Encoding.ASCII.GetBytes(headersBlock);

        //send headers and body
        await networkStream.WriteAsync(headerBytes, cancellationToken);
        if (bodyBytes.Length > 0)
        {
            await networkStream.WriteAsync(bodyBytes, cancellationToken);
        }
    }

    public async Task WriterRaw400Response(NetworkStream networkStream, CancellationToken cancellationToken = default)
    {
        var response = "HTTP/1.1 400 Bad Request\r\nContent-Length: 0\r\nConnection: close\r\n\r\n";
        var bytes = Encoding.ASCII.GetBytes(response);

        await networkStream.WriteAsync(bytes, cancellationToken);
    }
}
