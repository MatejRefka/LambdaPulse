using LambdaPulse.Server.Http.Abstractions;

namespace LambdaPulse.Server.Http.Parsing;

public interface IRequestParser
{
    WebContext ParseHttpRequest(string httpRequest);
}
