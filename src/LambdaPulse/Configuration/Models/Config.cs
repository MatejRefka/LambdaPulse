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
    public required int MaxControlDataSizeBytes { get; init; }
    public required int MaxHeaderSizeBytes { get; init; }
    public required int MaxBodySizeBytes { get; init; }
    public required int RequestReadTimeoutMS { get; init; }
    public required int RequestExecutionTimeoutMS { get; init; }
    public required int ConnectionIdleTimeoutMS { get; init; }
    public required bool HttpsRedirectionEnabled { get; init; }
}
