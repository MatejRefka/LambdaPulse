using LambdaPulse.Server.Http.Abstractions;

namespace LambdaPulse.Server.Http.Parsing;

public interface IRequestParser
{
    public WebContext ParseHttpRequest(string httpRequest);
}
