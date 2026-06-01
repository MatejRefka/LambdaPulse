namespace LambdaPulse.Engine.Configuration.Models;

public sealed class Config
{
    public required ServerConfig ServerConfig { get; init; }
}

public sealed class ServerConfig
{
    public required string Address { get; init; }
    public required int Port { get; init; }
    public int BackLog { get; init; }
    public required int ReadTimeoutMS { get; init; }
    public required MiddlewareConfig MiddlewareConfig { get; init; }
}

public sealed class MiddlewareConfig
{
    public required RequestLimitsMiddleware RequestLimitsMiddleware { get; init; }
    public required IpBlocklistMiddleware IpBlocklistMiddleware { get; init; }
    public required ConnectionMiddleware ConnectionMiddleware { get; init; }
    public required HttpsRedirectionMiddleware HttpsRedirectionMiddleware { get; init; }
    public required HstsMiddleware HstsMiddleware { get; init; }
    public required SecurityMiddleware SecurityMiddleware { get; init; }
    public required SessionMiddleware SessionMiddleware { get; init; }
    public required StaticFilesMiddleware StaticFilesMiddleware { get; init; }
    public required SpaMiddleware SpaFallbackMiddleware { get; init; }
    public required CorsMiddleware CorsMiddleware { get; init; }
}

public sealed class RequestLimitsMiddleware
{
    public required int MaxControlDataSizeBytes { get; init; }
    public required int MaxHeaderSizeBytes { get; init; }
    public required int MaxBodySizeBytes { get; init; }
}

public sealed class ConnectionMiddleware
{
    public required int RequestExecutionTimeoutMS { get; init; }
}

public sealed class IpBlocklistMiddleware
{
    public HashSet<string> BlockedIpAddresses { get; init; } = new();
}

public sealed class HttpsRedirectionMiddleware
{
    public required bool IsEnabled { get; init; }
}

public sealed class HstsMiddleware
{
    public required int MaxAge { get; init; }
    public required bool IncludeSubDomains { get; init; }
    public required bool Preload { get; init; }
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

public sealed class SpaMiddleware
{
    public required string IndexPageRelativePath { get; init; }
}

public sealed class CorsMiddleware
{
    public required HashSet<string> AllowedOrigins { get; init; }
    public required bool AllowCredentials { get; init; }
    public required HashSet<string> ExposedHeaders { get; init; }
    public required HashSet<string> AllowedMethods { get; init; }
    public required HashSet<string> AllowedHeaders { get; init; }
    public required int PreflightMaxAgeSeconds { get; init; }
}
