namespace LambdaPulse.Configuration;

public sealed class Config
{
    public ServerConfig ServerConfig { get; init; } = new();
}

public sealed class ServerConfig
{
    public string Address { get; init; } = "127.0.0.1";
    public int Port { get; init; } = 8080;
    public int BackLog { get; init; } = 512; //Kestrel's default
    public int RequestReadTimeoutMS { get; init; } = 120_000; //2 minutes
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
    public int MaxControlDataSizeBytes { get; init; } = 8_192; //8KB, Kestrel's default
    public int MaxHeaderSizeBytes { get; init; } = 32_768; //32KB, Kestrel's default
    public int MaxBodySizeBytes { get; init; } = 3_000_000; //3MB, the body is loaded into memory (not streamed)
}

public sealed class ConnectionConfig
{
    public int? RequestExecutionTimeoutMS { get; init; } = null;
}

public sealed class IpBlocklistConfig
{
    public HashSet<string> BlockedIpAddresses { get; init; } = [];
}

public sealed class HttpsRedirectionConfig
{
    public bool IsEnabled { get; init; }
}

public sealed class HstsConfig
{
    public bool IsEnabled { get; set; }
    public int MaxAge { get; init; } = 31_536_000; //one year
    public bool IncludeSubDomains { get; init; }
    public bool Preload { get; init; }
}

public sealed class SecurityConfig
{
    public bool XContentTypeOptions { get; init; } = true;
    public string? ReferrerPolicy { get; init; } = "strict-origin-when-cross-origin";
    public string? PermissionsPolicy { get; init; }
    public string? CrossOriginOpenerPolicy { get; init; }
    public string? CrossOriginResourcePolicy { get; init; }
    public string? CrossOriginEmbedderPolicy { get; init; }
    public bool RemoveServerHeader { get; init; } = true;
}

public sealed class SessionConfig
{
    public int IdleTimeoutMinutes { get; init; } = 20;
    public int AbsoluteTimeoutMinutes { get; init; } = 720; //12 hours
    public bool CookieSecure { get; init; } = false;
}

public sealed class StaticFilesConfig
{
    public string? FileRootPath { get; init; }
}

public sealed class SpaConfig
{
    public string? IndexPageRelativePath { get; init; }
}

public sealed class CorsConfig
{
    public HashSet<string> AllowedOrigins { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public bool AllowCredentials { get; init; }
    public HashSet<string> ExposedHeaders { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> AllowedMethods { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> AllowedHeaders { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public int PreflightMaxAgeSeconds { get; init; } = 600; //10 minutes
}
