namespace LambdaPulse.Engine.Features.State.Cache;

public sealed class CachePolicy
{
    public bool Enabled { get; init; }

    //default to 60 seconds if enabled but no duration specified.
    public int DurationSeconds { get; init; } = 60;
}
