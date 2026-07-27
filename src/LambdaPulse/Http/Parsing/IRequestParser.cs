using LambdaPulse.Http.Abstractions;

namespace LambdaPulse.Http.Parsing;

public interface IRequestParser
{
    WebContext ParseHttpRequest(string httpRequest, DateTimeOffset requestStartTimestamp, string? remoteIpAddress);
}
