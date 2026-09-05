using LambdaPulse.Features.State.Cache;
using LambdaPulse.Http.Abstractions;

namespace LambdaPulse.Features.Routing;

/// <summary>
/// Endpoint matching the incoming request, set by the routing middleware.
/// </summary>
public sealed class Endpoint
{
    /// <summary>
    /// Request method.
    /// </summary>
    public required string Method { get; init; }

    /// <summary>
    /// Request path.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Path parameters extracted from the request path.
    /// </summary>
    public Dictionary<string, string> PathParameters { get; set; } = new();

    /// <summary>
    /// Function to execute for this endpoint.
    /// </summary>
    public required Func<WebContext, CancellationToken, Task> ApplicationFunction { get; init; }

    /// <summary>
    /// Indicates whether the endpoint allows anonymous access.
    /// </summary>
    public bool AllowAnonymous { get; init; }

    /// <summary>
    /// Required role to access the endpoint.
    /// </summary>
    public string? RequiredRole { get; init; }

    /// <summary>
    /// Cache policy for the endpoint.
    /// </summary>
    public CachePolicy? CachePolicy { get; init; }

    /// <summary>
    /// Indicates whether to skip CSRF validation for the endpoint.
    /// </summary>
    public bool SkipCsrf { get; init; }
}
