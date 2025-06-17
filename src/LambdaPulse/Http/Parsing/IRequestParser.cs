using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Services.Http
{
    public interface IRequestParser
    {
        public WebContext ParseHttpRequest(string httpRequest);
    }
}
