namespace LambdaPulse.Engine.Features.Routing;

public interface IEndpointRegistry
{
    Endpoint? GetEndpoint(string method, string path);
    void AddEndpoint(Endpoint endpoint);
}
