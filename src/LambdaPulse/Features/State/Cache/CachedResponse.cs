namespace LambdaPulse.Engine.Features.State.Cache;

public sealed class CachedResponse
{
    public required int StatusCode { get; init; }
    public required string ResponsePhrase { get; init; }
    public required byte[] Body { get; init; }
    public string? ContentType { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
}
