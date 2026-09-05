namespace LambdaPulse.Features.Routing;

/// <summary>
/// Defines operations for adding and retrieving endpoints. Implementations handle the store.
/// </summary>
public interface IEndpointRegistry
{
    /// <summary>
    /// Retrieves an endpoint for a given HTTP method and path.
    /// </summary>
    Endpoint? GetEndpoint(string method, string path);

    /// <summary>
    /// Adds an endpoint to the registry.
    /// </summary>
    void AddEndpoint(Endpoint endpoint);
}
