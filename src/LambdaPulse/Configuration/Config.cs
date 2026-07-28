namespace LambdaPulse.Configuration;

public sealed class Config
{
    public ServerConfig ServerConfig { get; init; } = new();
}

public sealed class ServerConfig
{
    public string Address { get; init; } = "127.0.0.1";
    public int Port { get; init; } = 8080;
    public int BackLog { get; init; } = 100;
    public int ReadTimeoutMS { get; init; } = 120_000;
    public MiddlewareConfig MiddlewareConfig { get; init; } = new();
}

public sealed class MiddlewareConfig
{
    public RequestLimitsConfig RequestLimitsConfig { get; init; } = new();
    public IpBlocklistConfig IpBlocklistConfig { get; init; } = new();
    public ConnectionConfig ConnectionConfig { get; init; } = new();
    public HttpsRedirectionConfig HttpsRedirectionConfig { get; init; } = new();
    public HstsConfig HstsConfig { get; init; } = new();
    public SecurityConfig SecurityConfig { get; init; } = new();
    public SessionConfig SessionConfig { get; init; } = new();
    public StaticFilesConfig StaticFilesConfig { get; init; } = new();
    public SpaConfig SpaFallbackConfig { get; init; } = new();
    public CorsConfig CorsConfig { get; init; } = new();
}

public sealed class RequestLimitsConfig
{
    public int MaxControlDataSizeBytes { get; init; } = 3_000_000;
    public int MaxHeaderSizeBytes { get; init; } = 15_000;
    public int MaxBodySizeBytes { get; init; } = 3_000_000;
}

public sealed class ConnectionConfig
{
    public int RequestExecutionTimeoutMS { get; init; } = 10_000;
}

public sealed class IpBlocklistConfig
{
    public HashSet<string> BlockedIpAddresses { get; init; } = new();
}

public sealed class HttpsRedirectionConfig
{
    public bool IsEnabled { get; init; }
}

public sealed class HstsConfig
{
    public int MaxAge { get; init; } = 63_072_000;
    public bool IncludeSubDomains { get; init; } = true;
    public bool Preload { get; init; }
}

public sealed class SecurityConfig
{
    public bool XContentTypeOptions { get; init; } = true;
    public string? ReferrerPolicy { get; init; } = "strict-origin-when-cross-origin";
    public string? PermissionsPolicy { get; init; } = "camera=(), microphone=(), geolocation=()";
    public string? CrossOriginOpenerPolicy { get; init; } = "same-origin";
    public string? CrossOriginResourcePolicy { get; init; } = "same-origin";
    public string? CrossOriginEmbedderPolicy { get; init; } = "require-corp";
    public bool RemoveServerHeader { get; init; } = true;
}

public sealed class SessionConfig
{
    public int IdleTimeoutMinutes { get; init; } = 20;
    public int AbsoluteTimeoutMinutes { get; init; } = 720;
    public bool CookieSecure { get; init; } = true;
}

public sealed class StaticFilesConfig
{
    public string FileRootPath { get; init; } = "";
}

public sealed class SpaConfig
{
    public string IndexPageRelativePath { get; init; } = "";
}

public sealed class CorsConfig
{
    public HashSet<string> AllowedOrigins { get; init; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "http://localhost:8081"
    };
    public bool AllowCredentials { get; init; } = true;
    public HashSet<string> ExposedHeaders { get; init; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "X-Request-Id"
    };
    public HashSet<string> AllowedMethods { get; init; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET",
        "POST",
        "PUT",
        "DELETE",
        "OPTIONS"
    };
    public HashSet<string> AllowedHeaders { get; init; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Content-Type",
        "X-CSRF-Token"
    };
    public int PreflightMaxAgeSeconds { get; init; } = 60;
}
