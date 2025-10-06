namespace LambdaPulse.Configuration.Models;

public sealed class Config
{
    public required ServerConfig ServerConfig { get; init; }
}

public sealed class ServerConfig
{
    public required string Address { get; init; }
    public required int Port { get; init; }
    public int BackLog { get; init; } = 100;
    public required MiddlewareConfig MiddlewareConfig { get; init; }
}

public sealed class MiddlewareConfig
{
    public required string MaxControlDataSizeBytes { get; init; }
    public required string MaxHeaderCount { get; init; }
    public required string MaxHeaderSizeBytes { get; init; }
    public required string MaxBodySizeBytes { get; init; }
    public required string RequestReadTimeoutLimit { get; init; }
}
