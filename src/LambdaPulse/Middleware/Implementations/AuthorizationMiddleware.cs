using LambdaPulse.Server.Http.Abstractions;
using LambdaPulse.Server.Shared.Extensions;

namespace LambdaPulse.Server.Middleware.Implementations;

internal sealed class AuthorizationMiddleware : MiddlewareBase
{
    public AuthorizationMiddleware(Func<WebContext, CancellationToken, Task> nextFunction) : base(nextFunction)
    {
        _nextFunction = nextFunction;
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        //no endpoint so nothing to authorize
        if (webContext.Endpoint == null)
        {
            await _nextFunction(webContext, cancellationToken);
            return;
        }

        //public endpoint so continue to downstream middleware
        if (webContext.Endpoint.AllowAnonymous)
        {
            await _nextFunction(webContext, cancellationToken);
            return;
        }

        //user is not authenticated
        if (!webContext.User.IsAuthenticated)
        {
            webContext.WebResponse.StatusCode = 401;
            webContext.WebResponse.ResponsePhrase = "Unauthorized";
            webContext.WebResponse.Headers["Content-Type"] = "text/plain; charset=utf-8";
            await webContext.WebResponse.WriteStringToBody("User authentication is required to access this resource.", cancellationToken);
            return;
        }

        //role is required for the endpoint
        if (!string.IsNullOrWhiteSpace(webContext.Endpoint.RequiredRole))
        {
            var userHasRole = webContext.User.Roles.Any(role => string.Equals(role, webContext.Endpoint.RequiredRole, StringComparison.OrdinalIgnoreCase));

            //user does not have the role needed to access the endpoint
            if (!userHasRole)
            {
                webContext.WebResponse.StatusCode = 403;
                webContext.WebResponse.ResponsePhrase = "Forbidden";
                webContext.WebResponse.Headers["Content-Type"] = "text/plain; charset=utf-8";
                await webContext.WebResponse.WriteStringToBody("User is not authorized to access this resource.", cancellationToken);
                return;
            }
        }

        //user is authorized, continue to downstream middleware
        await _nextFunction(webContext, cancellationToken);

    }
}
