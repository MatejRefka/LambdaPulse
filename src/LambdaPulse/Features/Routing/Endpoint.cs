using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Services.Http.Routing;

public class Endpoint
{
    public required string Method { get; init; }
    public required string Path { get; init; }
    public required Func<WebContext, CancellationToken, Task> ApplicationFunction { get; init; }
}
