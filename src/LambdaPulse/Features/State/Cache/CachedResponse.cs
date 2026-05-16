namespace LambdaPulse.Engine.Features.State.Cache;

public sealed class CachedResponse
{
    public required int StatusCode { get; init; }
    public required string ResponsePhrase { get; init; }
    public required Dictionary<string, string> Headers { get; init; }
    public required byte[] Body { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
}
