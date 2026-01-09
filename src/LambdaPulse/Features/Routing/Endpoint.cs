using LambdaPulse.Server.Services.Http.Models;

namespace LambdaPulse.Server.Services.Http.Routing;

public class Endpoint
{
    public required string Method { get; init; }
    public required string Path { get; init; }
    public Dictionary<string, string> PathParameters { get; set; } = new();
    public required Func<WebContext, CancellationToken, Task> ApplicationFunction { get; init; }
}
