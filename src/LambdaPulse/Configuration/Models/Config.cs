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
    public required int HstsMaxAge { get; init; }
    public required bool HstsIncludeSubDomains { get; init; }
    public required bool HstsPreload { get; init; }
    public required SecurityMiddleware SecurityMiddleware { get; init; }
    public required SessionMiddleware SessionMiddleware { get; init; }
    public required StaticFilesMiddleware StaticFilesMiddleware { get; init; }
}

public sealed class SecurityMiddleware
{
    public required bool XContentTypeOptions { get; init; }
    public required string? ReferrerPolicy { get; init; }
    public required string? PermissionsPolicy { get; init; }
    public required string? CrossOriginOpenerPolicy { get; init; }
    public required string? CrossOriginResourcePolicy { get; init; }
    public required string? CrossOriginEmbedderPolicy { get; init; }
    public required bool RemoveServerHeader { get; init; }
}

public sealed class SessionMiddleware
{
    public required int IdleTimeoutMinutes { get; init; }
    public required int AbsoluteTimeoutMinutes { get; init; }
}

public sealed class StaticFilesMiddleware
{
    public required string FileRootPath { get; init; }
}
