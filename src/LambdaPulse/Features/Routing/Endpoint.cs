using LambdaPulse.Server.Http.Abstractions;

namespace LambdaPulse.Server.Features.Routing;

public sealed class Endpoint
{
    public required string Method { get; init; }
    public required string Path { get; init; }
    public Dictionary<string, string> PathParameters { get; set; } = new();
    public required Func<WebContext, CancellationToken, Task> ApplicationFunction { get; init; }
}
