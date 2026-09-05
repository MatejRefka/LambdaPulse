namespace LambdaPulse.Http.Abstractions;

/// <summary>
/// HTTP response representation.
/// </summary>
public sealed class WebResponse
{
    /// <summary>
    /// Response status code.
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// Response phrase.
    /// </summary>
    public string? ResponsePhrase { get; set; }

    /// <summary>
    /// Response headers.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Request cookie header.
    /// Holds key-value pair plus metadata. E.g. Set-Cookies: sessionId=123; Path=/; SameSite=Strict
    /// </summary>
    public List<string> Cookies { get; } = new();

    /// <summary>
    /// Response body as a memory stream.
    /// </summary>
    internal MemoryStream Body { get; } = new();

    /// <summary>
    /// Indicates whether the response has a body to be sent to the client.
    /// </summary>
    public bool HasBody { get; internal set; }

    /// <summary>
    /// Output stream for writing the response in real-time to the client (SSE).
    /// </summary>
    internal Stream? OutputStream { get; set; }

    /// <summary>
    /// Indicates whether the response is being sent in real-time to the client (SSE).
    /// </summary>
    public bool IsStreaming { get; internal set; }

    /// <summary>
    /// Indicates whether the response has started being sent to the client (headers sent).
    /// </summary>
    public bool HasStarted { get; internal set; }

}