namespace LambdaPulse.Configuration;

/// <summary>
/// Provides the root configuration for a LambdaPulse server.
/// </summary>
public sealed class Config
{
    /// <summary>
    /// Configures the server listener and HTTP middleware pipeline.
    /// </summary>
    public ServerConfig ServerConfig { get; init; } = new();
}

/// <summary>
/// Configures the network listener and HTTP middleware pipeline.
/// </summary>
public sealed class ServerConfig
{
    /// <summary>
    /// Selects the local IP address on which the server listens for connections.
    /// Format: an IPv4 or IPv6 address.
    /// Default: <c>127.0.0.1</c>.
    /// </summary>
    public string Address { get; init; } = "127.0.0.1";

    /// <summary>
    /// Selects the TCP port on which the server listens for connections.
    /// Default: <c>8080</c>.
    /// </summary>
    public int Port { get; init; } = 8080;

    /// <summary>
    /// Limits the number of pending TCP connections that the operating system may queue.
    /// Default: <c>512</c>.
    /// </summary>
    public int BackLog { get; init; } = 512;

    /// <summary>
    /// Limits how long the server waits to read a complete request from a connection.
    /// Default: <c>120000</c> milliseconds (2 minutes).
    /// </summary>
    public int RequestReadTimeoutMS { get; init; } = 120_000;

    /// <summary>
    /// Configures the HTTP middleware pipeline.
    /// </summary>
    public MiddlewareConfig MiddlewareConfig { get; init; } = new();
}

/// <summary>
/// Groups the configuration for each built-in HTTP middleware component.
/// </summary>
public sealed class MiddlewareConfig
{
    /// <summary>
    /// Limits the amount of request control data, header data, and body data accepted by the server.
    /// </summary>
    public RequestLimitsConfig RequestLimitsConfig { get; init; } = new();

    /// <summary>
    /// Blocks requests originating from configured IP addresses.
    /// </summary>
    public IpBlocklistConfig IpBlocklistConfig { get; init; } = new();

    /// <summary>
    /// Configures request execution timeouts and connection response behavior.
    /// </summary>
    public ConnectionConfig ConnectionConfig { get; init; } = new();

    /// <summary>
    /// Configures redirects from HTTP to HTTPS.
    /// </summary>
    public HttpsRedirectionConfig HttpsRedirectionConfig { get; init; } = new();

    /// <summary>
    /// Configures HTTP Strict Transport Security for HTTPS responses.
    /// </summary>
    public HstsConfig HstsConfig { get; init; } = new();

    /// <summary>
    /// Configures security-related response headers.
    /// </summary>
    public SecurityConfig SecurityConfig { get; init; } = new();

    /// <summary>
    /// Configures session lifetime and cookie transport security.
    /// </summary>
    public SessionConfig SessionConfig { get; init; } = new();

    /// <summary>
    /// Configures the directory from which static files are served.
    /// </summary>
    public StaticFilesConfig StaticFilesConfig { get; init; } = new();

    /// <summary>
    /// Configures the fallback document for single-page applications.
    /// </summary>
    public SpaConfig SpaFallbackConfig { get; init; } = new();

    /// <summary>
    /// Configures which cross-origin browser requests are permitted.
    /// </summary>
    public CorsConfig CorsConfig { get; init; } = new();
}

/// <summary>
/// Configures limits for incoming HTTP request data.
/// </summary>
public sealed class RequestLimitsConfig
{
    /// <summary>
    /// Limits the combined UTF-8 size of the request method, path, and protocol version.
    /// Default: <c>8192</c> bytes (8 KiB).
    /// </summary>
    public int MaxControlDataSizeBytes { get; init; } = 8_192;

    /// <summary>
    /// Limits the combined UTF-8 size of all request header names and values.
    /// Default: <c>32768</c> bytes (32 KiB).
    /// </summary>
    public int MaxHeaderSizeBytes { get; init; } = 32_768;

    /// <summary>
    /// Limits the UTF-8 size of the request body loaded into memory by the server.
    /// Default: <c>3000000</c> bytes (3 MB).
    /// </summary>
    public int MaxBodySizeBytes { get; init; } = 3_000_000;
}

/// <summary>
/// Configures request execution and connection behavior.
/// </summary>
public sealed class ConnectionConfig
{
    /// <summary>
    /// Limits how long the request processing pipeline may execute before returning HTTP 408.
    /// Default: <c>null</c> (timeout disabled).
    /// </summary>
    public int? RequestExecutionTimeoutMS { get; init; } = null;
}

/// <summary>
/// Configures IP addresses that are prohibited from accessing the server.
/// </summary>
public sealed class IpBlocklistConfig
{
    /// <summary>
    /// IP addresses that are blocked from accessing the server.
    /// Format: exact IPv4 or IPv6 address strings.
    /// Default: an empty set.
    /// </summary>
    public HashSet<string> BlockedIpAddresses { get; init; } = [];
}

/// <summary>
/// Configures redirects from HTTP to HTTPS.
/// </summary>
public sealed class HttpsRedirectionConfig
{
    /// <summary>
    /// Redirects requests to the equivalent HTTPS URL.
    /// Default: <c>false</c>.
    /// </summary>
    public bool IsEnabled { get; init; }
}

/// <summary>
/// Configures the Strict-Transport-Security response header.
/// </summary>
public sealed class HstsConfig
{
    /// <summary>
    /// Adds HTTP Strict Transport Security to responses for requests forwarded as HTTPS.
    /// Default: <c>false</c>.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Controls how long browsers remember to access the host only through HTTPS.
    /// Default: <c>31536000</c> seconds (1 year).
    /// </summary>
    public int MaxAge { get; init; } = 31_536_000;

    /// <summary>
    /// Extends HTTP Strict Transport Security protection to every subdomain of the host.
    /// Default: <c>false</c>.
    /// </summary>
    public bool IncludeSubDomains { get; init; }

    /// <summary>
    /// Adds the HSTS preload directive so the host can be submitted to browser preload lists.
    /// Default: <c>false</c>.
    /// </summary>
    public bool Preload { get; init; }
}

/// <summary>
/// Configures security-related HTTP response headers.
/// </summary>
public sealed class SecurityConfig
{
    /// <summary>
    /// Prevents browsers from interpreting a response as a MIME type other than its declared content type.
    /// Default: <c>true</c>.
    /// </summary>
    public bool XContentTypeOptions { get; init; } = true;

    /// <summary>
    /// Controls how much referrer information browsers include with outgoing requests.
    /// Default: <c>"strict-origin-when-cross-origin"</c>.
    /// </summary>
    public string? ReferrerPolicy { get; init; } = "strict-origin-when-cross-origin";

    /// <summary>
    /// Controls which browser features may be used by the website and its embedded content.
    /// Default: <c>null</c> (header omitted).
    /// </summary>
    public string? PermissionsPolicy { get; init; }

    /// <summary>
    /// Controls whether the document shares a browsing context group with cross-origin documents.
    /// Default: <c>null</c> (header omitted).
    /// </summary>
    public string? CrossOriginOpenerPolicy { get; init; }

    /// <summary>
    /// Controls whether the document may load cross-origin resources.
    /// Default: <c>null</c> (header omitted).
    /// </summary>
    public string? CrossOriginResourcePolicy { get; init; }

    /// <summary>
    /// Controls whether the document may load cross-origin resources that require CORS.
    /// Default: <c>null</c> (header omitted).
    /// </summary>
    public string? CrossOriginEmbedderPolicy { get; init; }

    /// <summary>
    /// Removes the Server header from HTTP responses to reduce information disclosure.
    /// Default: <c>true</c>.
    /// </summary>
    public bool RemoveServerHeader { get; init; } = true;
}

/// <summary>
/// Configures session expiration and cookie transport security.
/// </summary>
public sealed class SessionConfig
{
    /// <summary>
    /// Maximum idle time for a session before it expires.
    /// Default: <c>20</c> minutes.
    /// </summary>
    public int IdleTimeoutMinutes { get; init; } = 20;

    /// <summary>
    /// Absolute maximum lifetime of a session in minutes (independent of activity).
    /// Default: <c>720</c> minutes (12 hours).
    /// </summary>
    public int AbsoluteTimeoutMinutes { get; init; } = 720;

    /// <summary>
    /// Controls whether session cookies are marked as secure and only sent over HTTPS connections.
    /// Default: <c>false</c>.
    /// </summary>
    public bool CookieSecure { get; init; } = false;
}

/// <summary>
/// Configures static file serving.
/// </summary>
public sealed class StaticFilesConfig
{
    /// <summary>
    /// Selects the root directory from which static files are served.
    /// Default: <c>null</c>.
    /// </summary>
    public string? FileRootPath { get; init; }
}

/// <summary>
/// Configures single-page application fallback behavior.
/// </summary>
public sealed class SpaConfig
{
    /// <summary>
    /// Selects the SPA index page to serve for requests that do not match a static file or API endpoint.
    /// Default: <c>null</c>.
    /// </summary>
    public string? IndexPageRelativePath { get; init; }
}

/// <summary>
/// Configures Cross-Origin Resource Sharing behavior.
/// </summary>
public sealed class CorsConfig
{
    /// <summary>
    /// Identifies origins allowed to make cross-origin requests to the server.
    /// Default: an empty set (no origins allowed).
    /// </summary>
    public HashSet<string> AllowedOrigins { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Allows browsers to include credentials (cookies, authorization headers) in cross-origin requests.
    /// Default: <c>false</c>.
    /// </summary>
    public bool AllowCredentials { get; init; }

    /// <summary>
    /// Controls which HTTP response headers are exposed to cross-origin requests.
    /// Default: an empty set (no additional headers exposed).
    /// </summary>
    public HashSet<string> ExposedHeaders { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Controls which HTTP methods are allowed for cross-origin requests.
    /// Default: an empty set (allow-methods header omitted).
    /// </summary>
    public HashSet<string> AllowedMethods { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Controls which HTTP request headers are allowed for cross-origin requests.
    /// Default: an empty set (allow-headers header omitted).
    /// </summary>
    public HashSet<string> AllowedHeaders { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Controls how long browsers may cache a successful CORS preflight response.
    /// Default: <c>600</c> seconds (10 minutes).
    /// </summary>
    public int PreflightMaxAgeSeconds { get; init; } = 600;
}
