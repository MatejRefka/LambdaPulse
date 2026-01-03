using LambdaPulse.Server.Services.Http.Models;

namespace LambdaPulse.Server.Services.Http;

public sealed class RequestParser : IRequestParser
{
    public WebContext ParseHttpRequest(string httpRequest)
    {
        if (string.IsNullOrEmpty(httpRequest))
        {
            throw new ArgumentException("HTTP Request string cannot be null or empty");
        }

        using var reader = new StringReader(httpRequest);

        var controlData = reader.ReadLine() ?? throw new InvalidOperationException("Missing HTTP control data");

        var controlDataItems = controlData.Split(' ', 3);
        if (controlDataItems.Length != 3)
        {
            throw new InvalidOperationException("Invalid HTTP control data");
        }

        var method = controlDataItems[0];
        var path = controlDataItems[1];
        var protocol = controlDataItems[2];

        var headers = new Dictionary<string, string>();
        string? header;
        while (!string.IsNullOrEmpty(header = reader.ReadLine()))
        {
            var separatorIndex = header.IndexOf(':');

            var headerName = header[..separatorIndex].Trim();
            var headerValue = header[(separatorIndex + 1)..].Trim();

            headers[headerName] = headerValue;
        }

        string? body = null;
        if (headers.TryGetValue("Content-Length", out var value) && int.TryParse(value, out var contentLength) && contentLength > 0)
        {
            var buffer = new char[contentLength];
            var charsRead = reader.ReadBlock(buffer, 0, contentLength);
            body = new string(buffer, 0, charsRead);
        }

        var request = new WebRequest()
        {
            Method = method,
            Path = path,
            Protocol = protocol,
            Headers = headers,
            Body = body
        };

        var response = new WebResponse();

        var webContext = new WebContext
        {
            WebRequest = request,
            WebResponse = response
        };

        return webContext;
    }
}
