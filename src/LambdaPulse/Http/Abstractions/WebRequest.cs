namespace LambdaPulse.Http.Abstractions;

public sealed class WebRequest
{
    public required string Method { get; set; }
    public required string Path { get; set; }
    public Dictionary<string, string> QueryParameters { get; set; } = new();
    public required string Protocol { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    //Request cookie header holds key-value only, no metadata. E.g. Cookie: sessionId=123; theme=dark
    public Dictionary<string, string> Cookies { get; set; } = new();
    public string? Body { get; set; }
}
