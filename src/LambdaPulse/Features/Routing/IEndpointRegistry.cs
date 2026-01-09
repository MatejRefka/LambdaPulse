namespace LambdaPulse.Server.Features.Routing;

public interface IEndpointRegistry
{
    public Endpoint? GetEndpoint(string method, string path);
    public void AddEndpoint(Endpoint endpoint);
}
