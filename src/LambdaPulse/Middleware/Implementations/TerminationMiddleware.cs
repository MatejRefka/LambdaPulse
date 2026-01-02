using LambdaPulse.Services.Http.Models;
using LambdaPulse.Utility.Extensions;

namespace LambdaPulse.Middleware.Implementations;

public sealed class EndpointMiddleware : MiddlewareBase
{
    public EndpointMiddleware(Func<WebContext, CancellationToken, Task> nextFunction) : base(nextFunction)
    {
        _nextFunction = nextFunction;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        if (!webContext.WebResponse.HasStarted)
        {
            webContext.WebResponse.StatusCode = 404;
            webContext.WebResponse.ResponsePhrase = "Not Found";
            await webContext.WebResponse.WriteToBody("Not Found");
        }
        else
        {
            webContext.WebResponse.StatusCode = 200;
            webContext.WebResponse.ResponsePhrase = "OK";
            await Task.CompletedTask;
        }
    }
}