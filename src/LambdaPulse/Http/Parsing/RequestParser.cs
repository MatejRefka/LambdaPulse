using LambdaPulse.Features.Logging;
using LambdaPulse.Http.Abstractions;
using LambdaPulse.Shared.Extensions;

namespace LambdaPulse.Http.Parsing;

internal sealed class RequestParser : IRequestParser
{
    public WebContext ParseHttpRequest(string httpRequest, DateTimeOffset requestStartTimestamp, string? remoteIpAddress)
    {
        if (string.IsNullOrWhiteSpace(httpRequest))
        {
            throw new ArgumentException("HTTP Request string cannot be null or empty.");
        }

        using var reader = new StringReader(httpRequest);

        var controlData = reader.ReadLine() ?? throw new InvalidOperationException("Missing HTTP control data.");

        var controlDataItems = controlData.Split(' ', 3);
        if (controlDataItems.Length != 3)
        {
            throw new InvalidOperationException("Invalid HTTP control data.");
        }

        var method = controlDataItems[0];
        var rawUrl = controlDataItems[1];
        var protocol = controlDataItems[2];

        //separate path and query parameters from raw URL
        var urlSegments = rawUrl.Split('?', 2);
        var path = urlSegments[0];
        var queryParameters = urlSegments.Length > 1 ? ParseQueryParameters(urlSegments[1]) : new Dictionary<string, string>();

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? header;
        while (!string.IsNullOrWhiteSpace(header = reader.ReadLine()))
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

        var webContext = new WebContext
        {
            RemoteIpAddress = remoteIpAddress,
            WebRequest = new WebRequest()
            {
                Method = method,
                Path = path,
                QueryParameters = queryParameters,
                Protocol = protocol,
                Headers = headers,
                Cookies = headers.ParseCookies(),
                Body = body
            },
            WebResponse = new WebResponse(),
            Trace = new Trace()
            {
                TimestampStart = requestStartTimestamp,
                RequestMethod = method,
                RequestPath = path,
                RequestProtocol = protocol
            }
        };

        return webContext;
    }

    private static Dictionary<string, string> ParseQueryParameters(string queryParams)
    {
        var queryParamsDict = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(queryParams))
        {
            return new Dictionary<string, string>();
        }

        //E.g. "id=5&active=true&page=2"
        var queryParamsArr = queryParams.Split('&');

        foreach (var queryParam in queryParamsArr)
        {
            var keyValue = queryParam.Split("=", 2);
            var key = keyValue[0];
            var value = keyValue.Length > 1 ? keyValue[1] : string.Empty;

            queryParamsDict[key] = value;
        }

        return queryParamsDict;
    }
}
