namespace LambdaPulse.Http.Abstractions;

/// <summary>
/// HTTP request representation.
/// </summary>
public sealed class WebRequest
{
    /// <summary>
    /// Request method.
    /// </summary>
    public required string Method { get; set; }

    /// <summary>
    /// Request path.
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// Query parameters extracted from the request URL.
    /// </summary>
    public Dictionary<string, string> QueryParameters { get; set; } = new();

    /// <summary>
    /// Protocol version of the request.
    /// </summary>
    public required string Protocol { get; set; }

    /// <summary>
    /// Request headers.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Request cookie header.
    /// Holds key-value only, no metadata. E.g. Cookie: sessionId=123; theme=dark
    /// </summary>
    public Dictionary<string, string> Cookies { get; set; } = new();

    /// <summary>
    /// Request body.
    /// </summary>
    public string? Body { get; set; }
}
