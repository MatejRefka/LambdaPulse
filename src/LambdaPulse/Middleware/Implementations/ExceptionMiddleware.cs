using LambdaPulse.Server.Http.Abstractions;
using LambdaPulse.Server.Shared.Extensions;

namespace LambdaPulse.Server.Middleware.Implementations;

/// <summary>
/// Wraps the pipeline in a try/catch, ensuring the whole server doesn't crash.
/// Exception is logged, generating 500 response.
/// Any unhandled exception type implementing Exception responds with 500.
/// </summary>
internal sealed class ExceptionMiddleware : MiddlewareBase
{
    public ExceptionMiddleware(Func<WebContext, CancellationToken, Task> nextFunction) : base(nextFunction)
    {
        _nextFunction = nextFunction;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        try
        {
            await _nextFunction(webContext, cancellationToken);
        }
        catch (SystemException ex)
        {
            Console.WriteLine($"Unhandled exception thrown within the pipeline: {ex}");

            //do not overwrite the response as it could be being written to
            if (!webContext.WebResponse.HasStarted)
            {
                webContext.WebResponse.StatusCode = 500;
                webContext.WebResponse.ResponsePhrase = "Internal Server Error";
                await webContext.WebResponse.WriteStringToBody("The server encountered an unexpected condition that prevented it from fulfilling the request.", cancellationToken);

                //clear response headers
                webContext.WebResponse.Headers = new Dictionary<string, string>()
                {
                    ["Content-Type"] = "text/plain; charset=utf-8"
                };
            }
        }
    }
}
