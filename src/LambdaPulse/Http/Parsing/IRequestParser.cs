using LambdaPulse.Engine.Http.Abstractions;

namespace LambdaPulse.Engine.Http.Parsing;

public interface IRequestParser
{
    WebContext ParseHttpRequest(string httpRequest, DateTimeOffset requestStartTimestamp);
}
