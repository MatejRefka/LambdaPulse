namespace LambdaPulse.Services.Http.Routing;

public class EndpointRegistry
{
    private readonly List<Endpoint> _endpoints = new();

    public Endpoint? GetEndpoint(string method, string path)
    {
        var endpoint = _endpoints.FirstOrDefault(e => string.Equals(e.Method, method, StringComparison.OrdinalIgnoreCase) && string.Equals(e.Path, path, StringComparison.OrdinalIgnoreCase));
        return endpoint;
    }

    public void AddEndpoint(Endpoint endpoint)
    {
        _endpoints.Add(endpoint);
    }
}
