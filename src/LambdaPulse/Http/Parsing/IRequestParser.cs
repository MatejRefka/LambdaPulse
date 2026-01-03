using LambdaPulse.Server.Services.Http.Models;

namespace LambdaPulse.Server.Services.Http;

public interface IRequestParser
{
    public WebContext ParseHttpRequest(string httpRequest);
}
