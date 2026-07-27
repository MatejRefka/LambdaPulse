using LambdaPulse.Features.State.Cache;
using LambdaPulse.Http.Abstractions;

namespace LambdaPulse.Features.Routing;

public sealed class Endpoint
{
    public required string Method { get; init; }
    public required string Path { get; init; }
    public Dictionary<string, string> PathParameters { get; set; } = new();
    public required Func<WebContext, CancellationToken, Task> ApplicationFunction { get; init; }
    public bool AllowAnonymous { get; init; }
    public string? RequiredRole { get; init; }
    public CachePolicy? CachePolicy { get; init; }
    public bool SkipCsrf { get; init; }
}
