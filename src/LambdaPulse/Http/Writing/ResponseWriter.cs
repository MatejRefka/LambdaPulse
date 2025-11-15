using LambdaPulse.Services.Http.Models;
using System.Globalization;
using System.Net.Sockets;
using System.Text;

namespace LambdaPulse.Services.Http;

public sealed class ResponseWriter : IResponseWriter
{
    public async Task WriteHttpResponse(NetworkStream networkStream, WebResponse webResponse)
    {
        var responseStatusLine = $"HTTP/1.1 {webResponse.StatusCode} {webResponse.ResponsePhrase}\r\n";

        var bodyBytes = webResponse.Body == null ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(webResponse.Body);

        //recommended response headers for http/1.1
        if (!webResponse.Headers.ContainsKey("Content-Length"))
        {
            webResponse.Headers["Content-Length"] = bodyBytes.Length > 0 ? bodyBytes.Length.ToString(CultureInfo.InvariantCulture) : "0";
        }
        if (!webResponse.Headers.ContainsKey("Content-Type"))
        {
            webResponse.Headers["Content-Type"] = "text/plain; charset=utf-8";
        }
        if (!webResponse.Headers.ContainsKey("Connection"))
        {
            webResponse.Headers["Connection"] = webResponse.Headers["Connection"] = "close";
        }

        //parse WebResponse headers into string
        string headersBlock = responseStatusLine;
        foreach (var header in webResponse.Headers)
        {
            headersBlock += $"{header.Key}: {header.Value}\r\n";
        }
        headersBlock += "\r\n";

        //encode headers
        var headerBytes = Encoding.ASCII.GetBytes(headersBlock);

        //send headers and body
        await networkStream.WriteAsync(headerBytes);
        if (bodyBytes.Length > 0)
        {
            await networkStream.WriteAsync(bodyBytes);
        }
    }
}
