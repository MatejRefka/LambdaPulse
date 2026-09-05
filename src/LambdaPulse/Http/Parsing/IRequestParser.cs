using LambdaPulse.Http.Abstractions;

namespace LambdaPulse.Http.Parsing;

/// <summary>
/// Parses the raw HTTP requests.
/// </summary>
public interface IRequestParser
{
    /// <summary>
    /// Parses the raw HTTP request string into a WebContext object.
    /// </summary>
    WebContext ParseHttpRequest(string httpRequest, DateTimeOffset requestStartTimestamp, string? remoteIpAddress);
}
