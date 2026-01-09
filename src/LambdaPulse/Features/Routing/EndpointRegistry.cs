namespace LambdaPulse.Server.Features.Routing;

internal sealed class EndpointRegistry : IEndpointRegistry
{
    private readonly List<Endpoint> _endpoints = new();
    private readonly IEndpointComparer _comparer;

    public EndpointRegistry(IEndpointComparer comparer)
    {
        _comparer = comparer;
    }

    public Endpoint? GetEndpoint(string method, string path)
    {
        foreach (var endpoint in _endpoints)
        {
            //method must match
            if (!string.Equals(endpoint.Method, method, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var pathsMatch = GetPathParameters(endpoint.Path, path, out var pathParameters);

            //requested path matches registered endpoint path
            if (pathsMatch)
            {
                //needs a fresh endpoint isntance because each request will have different path parameters
                return new Endpoint { Method = endpoint.Method, Path = endpoint.Path, ApplicationFunction = endpoint.ApplicationFunction, PathParameters = pathParameters };
            }
        }

        return null;
    }

    public void AddEndpoint(Endpoint endpoint)
    {
        //do not register duplicate endpoints
        var endpointExists = _endpoints.Any(e => string.Equals(e.Method, endpoint.Method, StringComparison.OrdinalIgnoreCase) && string.Equals(e.Path, endpoint.Path, StringComparison.OrdinalIgnoreCase));
        if (endpointExists)
        {
            return;
        }

        _endpoints.Add(endpoint);

        //sort to prioritize static paths over parameterized paths
        _endpoints.Sort(_comparer);
    }

    private static bool GetPathParameters(string registeredPath, string requestedPath, out Dictionary<string, string> pathParameters)
    {
        pathParameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        //E.g. registeredPath = "/api/users/{userId}/orders/{orderId}"
        var registeredPathSegments = registeredPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        //E.g. requestedPath = "/api/users/12345/orders/67890"
        var requestedPathSegments = requestedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        //no match if segment counts differ
        if (requestedPathSegments.Length != registeredPathSegments.Length)
        {
            return false;
        }

        for (int i = 0; i < registeredPathSegments.Length; i++)
        {
            if (registeredPathSegments[i].StartsWith("{", StringComparison.OrdinalIgnoreCase) && registeredPathSegments[i].EndsWith("}", StringComparison.OrdinalIgnoreCase))
            {
                var key = registeredPathSegments[i].Trim('{', '}');
                pathParameters[key] = requestedPathSegments[i];
            }
            // "/api/users/{userId}/orders/{orderId}" should not match "/api/admin/{userId}/orders/{orderId}"
            else if (!string.Equals(registeredPathSegments[i], requestedPathSegments[i], StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}
