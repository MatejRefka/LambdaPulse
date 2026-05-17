using LambdaPulse.Engine.Features.Logging;
using LambdaPulse.Engine.Http.Abstractions;
using LambdaPulse.Engine.Shared.Extensions;

namespace LambdaPulse.Engine.Middleware.Implementations;

internal sealed class AuthorizationMiddleware : MiddlewareBase
{
    protected override string MiddlewareName => "Authorization";

    public AuthorizationMiddleware(Func<WebContext, CancellationToken, Task> nextFunction) : base(nextFunction)
    {
    }

    public override async Task Invoke(WebContext webContext, CancellationToken cancellationToken = default)
    {
        var downstreamStart = DateTimeOffset.UtcNow;

        //no endpoint so nothing to authorize
        if (webContext.Endpoint == null)
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "No endpoint matched for the request. Skipping authorization." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //public endpoint so continue to downstream middleware
        if (webContext.Endpoint.AllowAnonymous)
        {
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "Endpoint allows anonymous access. Skipping authorization." });
            await _nextFunction(webContext, cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);
            return;
        }

        //user is not authenticated
        if (!webContext.User.IsAuthenticated)
        {
            webContext.WebResponse.StatusCode = 401;
            webContext.WebResponse.ResponsePhrase = "Unauthorized";
            webContext.WebResponse.Headers["Content-Type"] = "text/plain; charset=utf-8";
            await webContext.WebResponse.WriteStringToBody("User authentication is required to access this resource.", cancellationToken);
            RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.ShortCircuit, downstreamStart, new List<string> { "User is not authenticated." });
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
                RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.ShortCircuit, downstreamStart, new List<string> { "User does not have the required role to access the endpoint." });
                return;
            }
        }

        //user is authorized, continue to downstream middleware
        RecordTelemetry(webContext, FlowDirection.Downstream, ExecutionEvent.Success, downstreamStart, new List<string> { "User is authorized." });
        await _nextFunction(webContext, cancellationToken);
        RecordTelemetry(webContext, FlowDirection.Upstream, ExecutionEvent.Success, DateTimeOffset.UtcNow);

    }
}
